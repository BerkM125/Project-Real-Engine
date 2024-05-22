const express = require('express');
const router = express.Router();
const db = require('./db');

// Basic GET request
router.get('/', (req, res) => {
    res.send('Hello, world!');
});

router.get('/tryInsertion', async (req, res) => {
    // await db.createRoomWithName("VONK", {});
    // res.send("CREATED ROOM");
    let testRoom = {
        $set: {
        "roomID" : "VONK",
        "users" : {
            "Berkan" : {
                "username" : "Berkan",
                "id" : "Berkan",
                "xp" : 0,
                "hp": 100,
                "kinematics" : {
                    "hand": {
                        "wrist": "0, 10, 0",
                        "thumb-first": "0, 0, 0",
                        "thumb-second": "0, 0, 0",
                        "thumb-third": "0, 0, 0",
                        "index-first": "0, 0, 0",
                        "index-second": "0, 0, 0",
                        "index-third": "0, 0, 0",
                        "middle-first": "0, 0, 0",
                        "middle-second": "0, 0, 0",
                        "middle-third": "0, 0, 0",
                        "ring-first": "0, 0, 0",
                        "ring-second": "0, 0, 0",
                        "ring-third": "0, 0, 0",
                        "pinkie-first": "0, 0, 0",
                        "pinkie-second": "0, 0, 0",
                        "pinkie-third": "0, 0, 0"
                    },
                    "body" : {
                        "right-shoulder": "0, 0, 0",
                        "left-shoulder": "0, 0, 0",
                        "right-elbow": "0, 0, 0",
                        "left-elbow": "0, 0, 0",
                        "right-hip": "0, 0, 0",
                        "left-hip": "0, 0, 0"
                    }
                }
            }       
        }
    }};
    db.insertData("VONK", testRoom);
    res.send("Updated.");
});

router.get('/tryGetting', async (req, res) => {
    res.send(await db.getUserByName("VONK", "Berkan"));
});
// Additional routes can be added here

module.exports = router;
