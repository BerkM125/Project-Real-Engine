import cv2
import mediapipe as mp
import numpy as np
import socket
import os
import json

ROOT_DIR = os.path.dirname(__file__)
# Sample data for global variables
host, port = "127.0.0.1", 13000

send_data = ""
RESULT = None
hand_map = {}
pose_map = {}

# SOCK_STREAM means TCP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    sock.connect((host, port))
finally:
    print("connected")

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic

# Load hand mapping dictionary from JSON file
with open(os.path.join(ROOT_DIR, "./hand_mapping_dict.json")) as dict_file:
    hand_map = json.load(dict_file)

with open(os.path.join(ROOT_DIR, "./pose_mapping_dict.json")) as dict_file:
    pose_map = json.load(dict_file)

# Server socket connection simplifier
def ping_server():
    global host, port, send_data, sock
    try:
        sock.sendall(send_data.encode("utf-8"))
        response = sock.recv(1024).decode("utf-8")
        # print(response)
    finally:
        return
        # print("Done")
    
def ping_pose_to_client(pose_landmarks, left_landmarks, right_landmarks):
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

# Begin OpenCV video capture and holistic body pose estimation
cap = cv2.VideoCapture(0)
with mp_holistic.Holistic(
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

        # Draw landmark annotation on the image.
        image.flags.writeable = True
        image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

        new_img = np.zeros(shape=(image.shape), dtype=np.uint8)
        new_img.flags.writeable = True
        new_img[:] = (70, 70, 70)

        if type(results.left_hand_landmarks) is not type(None) or type(
            results.right_hand_landmarks
        ) is not type(None):
            send_data = ping_pose_to_client(
                results.pose_landmarks,
                results.left_hand_landmarks,
                results.right_hand_landmarks,
            )
            ping_server()
            send_data = ""

        # Draw the pose-related landmarks
        mp_drawing.draw_landmarks(
            new_img,
            results.pose_landmarks,
            mp_holistic.POSE_CONNECTIONS,
            landmark_drawing_spec=mp_drawing_styles.get_default_pose_landmarks_style(),
        )

        # Draw all hand-related landmarks
        # Left hand
        mp_drawing.draw_landmarks(
            new_img,
            results.left_hand_landmarks,
            mp_holistic.HAND_CONNECTIONS,
            landmark_drawing_spec=mp_drawing_styles.get_default_hand_landmarks_style(),
        )

        # Right hand
        mp_drawing.draw_landmarks(
            new_img,
            results.right_hand_landmarks,
            mp_holistic.HAND_CONNECTIONS,
            landmark_drawing_spec=mp_drawing_styles.get_default_hand_landmarks_style(),
        )

# process_results()
sock.close()
cap.release()
