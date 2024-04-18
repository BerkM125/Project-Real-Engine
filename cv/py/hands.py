# Convert footage of people's hands from image and, on the SINGLE, SYNCED THREAD, process the image for hand landmarking

# Import necessary modules
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from mediapipe import solutions
from mediapipe.framework.formats import landmark_pb2
import numpy as np  # for converting frame to array for MP Image object
import cv2 as cv  # for capturing livestream with camera

hand_model_path = './landmarkers/hand_landmarker.task'
RESULT = None
results = []

BaseOptions = mp.tasks.BaseOptions
HandLandmarker = mp.tasks.vision.HandLandmarker
HandLandmarkerOptions = mp.tasks.vision.HandLandmarkerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

# To annotate a frame
def draw_landmarks_on_image(rgb_image):
  results.append(RESULT)
  hand_landmarks_list = RESULT.hand_landmarks
  annotated_frame = np.copy(rgb_image)

  # Loop through the detected hands to visualize.
  for idx in range(len(hand_landmarks_list)):
    hand_landmarks = hand_landmarks_list[idx]

    # Draw the hand landmarks.
    hand_landmarks_proto = landmark_pb2.NormalizedLandmarkList()
    hand_landmarks_proto.landmark.extend([
      landmark_pb2.NormalizedLandmark(x=landmark.x, y=landmark.y, z=landmark.z) for landmark in hand_landmarks
    ])
    solutions.drawing_utils.draw_landmarks(
      annotated_frame,
      hand_landmarks_proto,
      solutions.hands.HAND_CONNECTIONS,
      solutions.drawing_styles.get_default_hand_landmarks_style())
  return annotated_frame

# Create a hand landmarker instance with the image mode:
options = HandLandmarkerOptions(
    base_options=BaseOptions(model_asset_path=hand_model_path),
    running_mode=VisionRunningMode.IMAGE,
    min_hand_presence_confidence=0.3,
    min_tracking_confidence=0.3,
    min_hand_detection_confidence=0.3,
    num_hands=2)

with HandLandmarker.create_from_options(options) as landmarker:
    # Start capturing from camera
    cam = cv.VideoCapture(0)  # index specifies camera (alternatively, can pass in video file name)
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
        if cv.waitKey(1) & 0xFF == ord('q'):
            break
        # Convert the frame received from OpenCV to a MediaPipe’s Image object.
        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=np.array(frame))
        RESULT = landmarker.detect(mp_image)
        # VISUALIZE DETECTION RESULT -- WIP
        if type(RESULT) is not type(None):
            annotated_frame = draw_landmarks_on_image(mp_image.numpy_view())
            cv.imshow('Frame', annotated_frame)

    cam.release()
    cv.destroyAllWindows()

print("num frames saved: ", len(results))    
    