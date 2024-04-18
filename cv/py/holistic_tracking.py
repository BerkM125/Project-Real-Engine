import cv2
import mediapipe as mp
import numpy as np

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic

IMAGE_FILES = []
RESULT_BUFFER = []
BG_COLOR = (70, 70, 70)

def process_results():
  global RESULT_BUFFER
  dump_file = open("./out/coordinate_dump.obj", "w")

  for res in RESULT_BUFFER:
    dump_file.write("# Pose coords: \n")
    if type(res.pose_landmarks) is not type(None):
        for idx in range(len(res.pose_landmarks.landmark)):
            curr_pose_lm = res.pose_landmarks.landmark[idx]
            pl = curr_pose_lm
            dump_file.write(str("v " + str(pl.x) + " " + str(pl.y) + " " + str(pl.z) + " 1.0\n"))

    if type(res.left_hand_landmarks) is not type(None):
        dump_file.write("# Left Hand coords: \n")
        for idx in range(len(res.left_hand_landmarks.landmark)):
            curr_hand_lm = res.left_hand_landmarks.landmark[idx]
            hl = curr_hand_lm
            dump_file.write(str("v " + str(hl.x) + " " + str(hl.y) + " " + str(hl.z) + " 1.0\n"))

    dump_file.write("# Right Hand coords: \n")

    if type(res.right_hand_landmarks) is not type(None):
        for idx in range(len(res.right_hand_landmarks.landmark)):
            curr_hand_lm = res.right_hand_landmarks.landmark[idx]
            hl = curr_hand_lm
            dump_file.write(str("v " + str(hl.x) + " " + str(hl.y) + " " + str(hl.z) + " 1.0\n"))

  dump_file.close()

cap = cv2.VideoCapture(0)
with mp_holistic.Holistic(    
    model_complexity=1,
    static_image_mode=False,
    enable_segmentation=True,
    refine_face_landmarks=False,
    min_detection_confidence=0.5,
    min_tracking_confidence=0.5) as holistic:
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
    #print(results)
    RESULT_BUFFER.append(results)

    # Draw landmark annotation on the image.
    image.flags.writeable = True
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)

    new_img = np.zeros(shape=(image.shape), dtype=np.uint8)
    new_img.flags.writeable = True
    new_img[:] = BG_COLOR

    # Draw the pose-related landmarks
    mp_drawing.draw_landmarks(
        new_img,
        results.pose_landmarks,
        mp_holistic.POSE_CONNECTIONS,
        landmark_drawing_spec=mp_drawing_styles
        .get_default_pose_landmarks_style())

    # Draw all hand-related landmarks
    # Left hand
    mp_drawing.draw_landmarks(
        new_img,
        results.left_hand_landmarks,
        mp_holistic.HAND_CONNECTIONS,
        landmark_drawing_spec=mp_drawing_styles
        .get_default_hand_landmarks_style())
    
    # Right hand
    mp_drawing.draw_landmarks(
        new_img,
        results.right_hand_landmarks,
        mp_holistic.HAND_CONNECTIONS,
        landmark_drawing_spec=mp_drawing_styles
        .get_default_hand_landmarks_style())

    cv2.imshow('Full Body Tracking', cv2.flip(new_img, 1))
    if cv2.waitKey(5) & 0xFF == 27:
      break

process_results()
cap.release()