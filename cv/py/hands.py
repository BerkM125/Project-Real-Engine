# Convert footage of people's hands from image and, on the SINGLE, SYNCED THREAD, process the image for hand landmarking

# Import necessary modules
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from mediapipe import solutions
from mediapipe.framework.formats import landmark_pb2
import json
import numpy as np  # for converting frame to array for MP Image object
import cv2 as cv  # for capturing livestream with camera
import socket

# Sample data for global variables
host, port = "127.0.0.1", 13000
send_data = ""

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    sock.connect((host, port))
finally:
    print("connected")

hand_model_path = "./landmarkers/hand_landmarker.task"
RESULT = None
hand_map = {}

BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

# Load hand mapping dictionary from JSON file
with open("hand_mapping_dict.json") as dict_file:
    hand_map = json.load(dict_file)


# Server socket connection simplifier
def ping_server():
    global host, port, send_data, sock
    try:
        sock.sendall(send_data.encode("utf-8"))
        response = sock.recv(1024).decode("utf-8")
        #print(response)
    finally:
        return
        #print("Done")


def invert_handedness(hn):
    return "left" if hn == "right" else "right"

# Send socket signal to game TCP listener for hand position control
def ping_landmarks_to_client(world_landmarks, handedness):
    global send_data
    temp_data = ""
    for idx in range(len(world_landmarks)):
        if type(world_landmarks[idx]) is type(None):
            continue
        for wdx in range(len(world_landmarks[idx])):
            pl = world_landmarks[idx][wdx]
            if str(wdx) not in hand_map:
                continue
            part = f"{invert_handedness(handedness[0][0].display_name.lower())}-{hand_map[str(wdx)]}";
            temp_data += f"{part}: {(pl.x)}, {(pl.y)}, {(pl.z)}"
            if (wdx < len(world_landmarks[idx])-1):
                temp_data += "|"

    send_data = temp_data     
    if(send_data != ""):   
        ping_server()      


# To annotate a frame
def draw_landmarks_on_image(rgb_image):
    hand_landmarks_list = RESULT.hand_landmarks
    annotated_frame = np.copy(rgb_image)

    # Loop through the detected hands to visualize.
    for idx in range(len(hand_landmarks_list)):
        hand_landmarks = hand_landmarks_list[idx]

        # Draw the hand landmarks.
        hand_landmarks_proto = landmark_pb2.NormalizedLandmarkList()
        hand_landmarks_proto.landmark.extend(
            [
                landmark_pb2.NormalizedLandmark(
                    x=landmark.x, y=landmark.y, z=landmark.z
                )
                for landmark in hand_landmarks
            ]
        )
        solutions.drawing_utils.draw_landmarks(
            annotated_frame,
            hand_landmarks_proto,
            solutions.hands.HAND_CONNECTIONS,
            solutions.drawing_styles.get_default_hand_landmarks_style(),
        )
    return annotated_frame


# Create a hand landmarker instance with the image mode:
options = HandLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=hand_model_path),
    running_mode=VisionRunningMode.IMAGE,
    min_hand_presence_confidence=0.3,
    min_tracking_confidence=0.3,
    min_hand_detection_confidence=0.3,
    num_hands=2,
)

with HandLandmarker.create_from_options(options) as landmarker:
    # Start capturing from camera
    cam = cv.VideoCapture(
        0
    )  # index specifies camera (alternatively, can pass in video file name)
    if not cam.isOpened():
        print("unable to open camera")

    # Create a loop to read the latest frame from the camera using VideoCapture#read()
    while True:
        # capture video frame
        ret, frame = cam.read()

        # check if frame was read correctly
        if not ret:
            print("error capturing frame")
            break

        # Get the timestamp of the frame
        timestamp = cam.get(cv.CAP_PROP_POS_MSEC)

        # to break out of loop when 'q' is pressed
        if cv.waitKey(1) & 0xFF == ord("q"):
            break
        # Convert the frame received from OpenCV to a MediaPipe’s Image object.
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=np.array(frame))
        RESULT = landmarker.detect(mp_image)

        if type(RESULT.hand_landmarks) is not type(None):
            hw_marks = RESULT.hand_landmarks
            ping_landmarks_to_client(hw_marks, RESULT.handedness)
            annotated_frame = draw_landmarks_on_image(mp_image.numpy_view())
            cv.imshow("Frame", cv.flip(annotated_frame, 1))

    cam.release()
    cv.destroyAllWindows()
