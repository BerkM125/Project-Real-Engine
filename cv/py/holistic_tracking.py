# 
# holistic_tracking.py - Berkan Mertan
# This Python script is in charge of establishing a socket connection with the Unity clientside
# and managing it via a local TCP listener on one thread, and also using OpenCV and MediaPipe to
# process player's webcam footage via deep learning and bundle their coordinate data (in 3D space)
# into custom string format for use on the Unity client side.
#

# Import all dependencies
import cv2
import mediapipe as mp
import os
import json
import asyncio
import socketio
import threading
import uvicorn

ROOT_DIR = os.path.dirname(__file__)

# Global variable that stores all our stringified kinematic data
send_data = ""

# Dictionaries / maps for mapping MediaPipe's output data to fit the schema
# of the JSON used in our database and in serializable Unity components.
hand_map = {}
pose_map = {}

# Load the mediapipe solutions and utilities for holistic landmarking
mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic

# Load the trained gesture recogntition neural networks
gesture_path = './landmarkers/gesture_recognizer.task'

# Specify further options
BaseOptions = mp.tasks.BaseOptions
GestureRecognizer = mp.tasks.vision.GestureRecognizer
GestureRecognizerOptions = mp.tasks.vision.GestureRecognizerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

# Create a gesture recognizer instance with the image mode:
gesture_options = GestureRecognizerOptions(
    base_options=BaseOptions(model_asset_path=gesture_path),
    running_mode=VisionRunningMode.IMAGE)

# Keep track of previous attacks to prevent SPAMMING (this is sort of unnecessary)
prev_attack = ""

# Fun part: map the MediaPipe gesture recognition outputs to attack type
attack_map = {
   "Open_Palm" : "BLAST AWAY!!",
   "Thumb_Up" : "LIGHTING!!",
   "Closed_Fist" : "LIGHTNING!!",
   "Victory" : "FIRE!!",
   "Pointing_Up" : "RED!!",
   "ILoveYou" : "WEB!!"
}

# Load hand mapping dictionary from JSON file
with open(os.path.join(ROOT_DIR, "./hand_mapping_dict.json")) as dict_file:
    hand_map = json.load(dict_file)

with open(os.path.join(ROOT_DIR, "./pose_mapping_dict.json")) as dict_file:
    pose_map = json.load(dict_file)

# Create a TCP listener running socketio
# This will communicate with the socket client
# on the Unity end, thereby creating a BRIDGE between
# data on our Python end to data on our Unity end.
sio = socketio.AsyncServer(async_mode='asgi')
app = socketio.ASGIApp(sio)

# List of connected clients to our server.
clients = set()

# Async task, emit our data to Unity client
async def ping_unity_client():
    global sio, send_data
    try:
        await sio.emit("post-tracking-update", send_data)
    finally:
        return

# Async task and function for preparing all our landmark result data into a socket-transferrable format
async def prepare_results_for_client(pose_landmarks, left_landmarks, right_landmarks):
    temp_data = ""

    # Now add pose landmark data to the large socket stream string
    if type(pose_landmarks) is not type(None):
        for idx in range(len(pose_landmarks.landmark)):
            pl = pose_landmarks.landmark[idx]

            # Check presence in relevant JSON map
            if str(idx) not in pose_map:
                continue
            # Take index of the landmark to identify the body part
            # Take the body part's coordinates and add it to the string
            part = f"{pose_map[str(idx)]}"
            temp_data += f"{part}: {(-pl.x)}, {(pl.y)}, {(pl.z)}"

            # Prevent a parsing error with a || duplication or | at EOL
            if idx < len(pose_landmarks.landmark) - 1:
                temp_data += "|"     

    # Now add left landmark data to the large socket stream string
    if type(left_landmarks) is not type(None):
        for idx in range(len(left_landmarks.landmark)):
            hl = left_landmarks.landmark[idx]

            # Check presence in relevant JSON map
            if str(idx) not in hand_map:
                continue
            # Take index of the landmark to identify the body part
            # Take the body part's coordinates and add it to the string
            part = f"right-{hand_map[str(idx)]}"
            temp_data += f"{part}: {(-hl.x)}, {(hl.y)}, {(hl.z)}"

            # Prevent a parsing error with a || duplication or | at EOL
            if idx < len(left_landmarks.landmark) - 1:
                temp_data += "|"

    # Now add right landmark data to the large socket stream string
    if type(right_landmarks) is not type(None):
        for idx in range(len(right_landmarks.landmark)):
            hl = right_landmarks.landmark[idx]

            # Check presence in relevant JSON map
            if str(idx) not in hand_map:
                continue
            # Take index of the landmark to identify the body part
            # Take the body part's coordinates and add it to the string
            part = f"left-{hand_map[str(idx)]}"
            temp_data += f"{part}: {(-hl.x)}, {(hl.y)}, {(hl.z)}"

            # Prevent a parsing error with a || duplication or | at EOL
            if idx < len(right_landmarks.landmark) - 1:
                temp_data += "|"

    # Return the string, now formatted perfectly
    return temp_data


# The asynchronous task and function handling ALL holistic tracking and gesture recognition.
# This function creates a video capture instance using OpenCV, executes some basic image preprocessing
# to prepare the frame for MediaPipe, then sends the image through the holistic landmarking neural network
# as well as the gesture recognition neural network. It then passes the results obtained into appropriate
# functions and emits a message to the Unity client with the player's pose data.
async def pose_tracking_handler():
    global mp_holistic, send_data, prev_attack
    # Begin OpenCV video capture and holistic body pose estimation
    cap = cv2.VideoCapture(0)

    # Model doesn't have to be too complex, confidence can be ~50% for some lower quality cameras
    # No need to refine face landmarks, image is not static.
    with GestureRecognizer.create_from_options(gesture_options) as moveRecognizer, \
    mp_holistic.Holistic(
        model_complexity=1,
        static_image_mode=False,
        enable_segmentation=True,
        refine_face_landmarks=False,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    ) as holistic:
        # OpenCV loop
        while cap.isOpened():
            success, image = cap.read()
        
            if not success:
                print("Ignoring empty camera frame.")
                # If loading a video, use 'break' instead of 'continue'.
                continue

            # To improve performance, optionally mark the image as not writeable to
            # pass by reference.
            image.flags.writeable = False
            image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
            results = holistic.process(image)

            # Get our gesture result from the model
            gesture_recognition_result = moveRecognizer.recognize(mp.Image(image_format=mp.ImageFormat.SRGB, data=image))

            # Init vars
            attackInfoConcat = ""
            attackNameSave = ""

            # If gestures were detected, convert gesture to attack type, concatenate attack type
            # to be eventually passed to our send_data variable, for transfer of data to Unity
            if gesture_recognition_result.gestures:
                move = gesture_recognition_result.gestures[0][0].category_name
                handedness = gesture_recognition_result.handedness[0][0].category_name

                # Avoid keyerrors...
                if (attack_map.__contains__(move)):
                    attackNameSave = attack_map[move]
                    attackInfoConcat = f"attack:{attackNameSave},{'right' if (handedness.lower() == 'left') else 'left'}"

            # Make sure EITHER hand is detected
            if type(results.left_hand_landmarks) is not type(None) or type(
                results.right_hand_landmarks
            ) is not type(None):
                # Wait for PROPERLY FORMATTED DATA to be FULLY PREPARED, then load it into send_data
                send_data = await prepare_results_for_client(
                    results.pose_landmarks,
                    results.left_hand_landmarks,
                    results.right_hand_landmarks,
                )

                # If the previous attack is NOT the current attack, send the attack to Unity to trigger an attack
                if prev_attack is not attackNameSave:
                    send_data += attackInfoConcat
                # Set prev attack
                prev_attack = attackNameSave

                # Ping our Unity client-side socket listener!
                await ping_unity_client()
                send_data = ""

    # Release OpenCV instance
    cap.release()

# Track socketio connections to our TCP listener

# Connection callback
@sio.event
async def connect(sid, environ):
    print('Client connected:', sid)

    # This is sorta how SocketIO manages its room system too...
    clients.add(sid)

# Connection callback
@sio.event
async def disconnect(sid):
    print('Client disconnected:', sid)
    clients.remove(sid)

# NOT an asynchronous task, will run on another thread
def run_server():
    uvicorn.run(app, host='localhost', port=8000)

# Async task, create the pose tracking task and run it,
# also run the server and socket code on another thread.
async def main():
    asyncio.create_task(pose_tracking_handler())
    threading.Thread(target=run_server, daemon=True).start()

# Initiate
if __name__ == '__main__':
    asyncio.run(main())