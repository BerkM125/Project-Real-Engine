import cv2
import mediapipe as mp
import numpy as np
import socket
import os
import json
import asyncio
import socketio
import threading
import uvicorn

ROOT_DIR = os.path.dirname(__file__)

send_data = ""
frame_success = False
frame_image = np.zeros((100, 100, 3), dtype=np.uint8)

RESULT = None
hand_map = {}
pose_map = {}

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic

gesture_path = './landmarkers/gesture_recognizer.task'

BaseOptions = mp.tasks.BaseOptions
GestureRecognizer = mp.tasks.vision.GestureRecognizer
GestureRecognizerOptions = mp.tasks.vision.GestureRecognizerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

# Create a gesture recognizer instance with the image mode:
gesture_options = GestureRecognizerOptions(
    base_options=BaseOptions(model_asset_path=gesture_path),
    running_mode=VisionRunningMode.IMAGE)
cap = cv2.VideoCapture(0)

prev_attack = ""
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

# Create a Socket.IO server
sio = socketio.AsyncServer(async_mode='asgi')
app = socketio.ASGIApp(sio)

# List of connected clients
clients = set()

async def ping_server():
    global sio, send_data
    try:
        await sio.emit("post-tracking-update", send_data)
    finally:
        return
    
async def ping_pose_to_client(pose_landmarks, left_landmarks, right_landmarks):
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

    return temp_data

async def pose_tracking_handler():
    global mp_holistic, send_data, frame_success, frame_image, prev_attack
    # Begin OpenCV video capture and holistic body pose estimation
    cap = cv2.VideoCapture(0)
    with GestureRecognizer.create_from_options(gesture_options) as moveRecognizer, \
    mp_holistic.Holistic(
        model_complexity=1,
        static_image_mode=False,
        enable_segmentation=True,
        refine_face_landmarks=False,
        min_detection_confidence=0.5,
        min_tracking_confidence=0.5,
    ) as holistic:
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

            gesture_recognition_result = moveRecognizer.recognize(mp.Image(image_format=mp.ImageFormat.SRGB, data=image))

            attackInfoConcat = ""
            attackNameSave = ""

            if gesture_recognition_result.gestures:
                move = gesture_recognition_result.gestures[0][0].category_name
                handedness = gesture_recognition_result.handedness[0][0].category_name
                if (attack_map.__contains__(move)):
                    attackNameSave = attack_map[move]
                    attackInfoConcat = f"attack:{attackNameSave},{'right' if (handedness.lower() == 'left') else 'left'}"

            if type(results.left_hand_landmarks) is not type(None) or type(
                results.right_hand_landmarks
            ) is not type(None):
                send_data = await ping_pose_to_client(
                    results.pose_landmarks,
                    results.left_hand_landmarks,
                    results.right_hand_landmarks,
                )
                if prev_attack is not attackNameSave:
                    send_data += attackInfoConcat

                prev_attack = attackNameSave
                await ping_server()
                send_data = ""
    cap.release()

@sio.event
async def connect(sid, environ):
    print('Client connected:', sid)
    clients.add(sid)

@sio.event
async def disconnect(sid):
    print('Client disconnected:', sid)
    clients.remove(sid)

def run_server():
    uvicorn.run(app, host='localhost', port=8000)

async def main():
    asyncio.create_task(pose_tracking_handler())
    threading.Thread(target=run_server, daemon=True).start()

if __name__ == '__main__':
    asyncio.run(main())