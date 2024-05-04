# Project-Real-Engine
Official repository for the Real Engine (official name pending) hands-free, movement controlled multiplayer platform.

## Timeline

**04/20/2024:** Basic upper-body game control via the camera is established. Real-world landmarkers for hands still need to be applied correctly (MediaPipe's "holistic" system is outdated and will not support this, will require an independent HandLandmarkerResult to work). Scaling the coordinates and correctly positioning them according to each body part's origin in the game and in real life needs to be improved (e.g wrist and elbow positioning may be good, but shoulder motion should be more sensitive to changes in body position).

**05/03/2024:** Temporarily removed pose estimation alone to focus on hand tracking. Hand landmarkers are perfectly translated into the world and bone/flesh is simulated with the use of cylindrical/ellipsoidical wedges connecting the joints of the hands. Socket communication of hand coordinates is optimized now to send all 30ish joints at the same time to the Unity TCP listener.