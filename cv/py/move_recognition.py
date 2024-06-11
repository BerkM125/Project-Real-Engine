# 
# move_recognition.py - Berkan Mertan
# This Python script is not run in the main program, just a testing file for the MediaPipe gesture recognition.
#


import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
import cv2

model_path = './landmarkers/gesture_recognizer.task'

BaseOptions = mp.tasks.BaseOptions
GestureRecognizer = mp.tasks.vision.GestureRecognizer
GestureRecognizerOptions = mp.tasks.vision.GestureRecognizerOptions
VisionRunningMode = mp.tasks.vision.RunningMode

# Create a gesture recognizer instance with the image mode:
options = GestureRecognizerOptions(
    base_options=BaseOptions(model_asset_path=model_path),
    running_mode=VisionRunningMode.IMAGE)
cap = cv2.VideoCapture(0)

attack_map = {
   "Open_Palm" : "BLAST AWAY!!",
   "Thumb_Up" : "LIGHTING!!",
   "Closed_Fist" : "LIGHTNING!!",
   "Victory" : "FUUGA!",
   "Pointing_Up" : "AKA!!",
   "ILoveYou" : "WEB!!"
}

with GestureRecognizer.create_from_options(options) as recognizer:
  # The detector is initialized. Use it here.

  # Load the input image from an image file.
  while (cap.isOpened()):
    success, image = cap.read()

    if not success:
        print("Ignoring empty camera frame.")
        # If loading a video, use 'break' instead of 'continue'.
        continue

    # To improve performance, optionally mark the image as not writeable to
    # pass by reference.
    image.flags.writeable = False
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    gesture_recognition_result = recognizer.recognize(mp.Image(image_format=mp.ImageFormat.SRGB, data=image))

    if gesture_recognition_result.gestures:
       move = gesture_recognition_result.gestures[0][0].category_name
       if (attack_map.__contains__(move)):
          print(attack_map[move])