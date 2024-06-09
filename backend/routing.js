const express = require('express');
const router = express.Router();
const db = require('./db');
const io = require('./index.js');

// Basic GET request
router.get('/', (req, res) => {
    res.send('Hello, world!');
});

router.get('/tryInsertion', async (req, res) => {
    // await db.createRoomWithName("VONK", {});
    // res.send("CREATED ROOM");

    db.createRoomWithName("VONK");
    res.send("Updated.");
});

router.get('/tryGetting', async (req, res) => {
    res.send(await db.getUserByName("VONK", "Berkan"));
});
// Additional routes can be added here

router.get('/getKinematicData', async (req, res) => {
    res.send("Trying...");
    //io.to("VONK").emit("direct-get-data", "random");
});

router.get('/getSampleJSON', async (req, res) => {
    let testRoom = {
        "username" : "Berkan",
        "id" : "Berkan",
        "xp" : 0,
        "hp": 100,
        "kinematics" : {
            "hand": {
                "right-wrist": "0, 10, 0",
                "right-thumb-first": "0, 10, 0",
                "right-thumb-second": "0, 10, 0",
                "right-thumb-third": "0, 10, 0",
                "right-index-first": "0, 10, 0",
                "right-index-second": "0, 10, 0",
                "right-index-third": "0, 10, 0",
                "right-middle-first": "0, 10, 0",
                "right-middle-second": "0, 10, 0",
                "right-middle-third": "0, 10, 0",
                "right-ring-first": "0, 10, 0",
                "right-ring-second": "0, 10, 0",
                "right-ring-third": "0, 10, 0",
                "right-pinkie-first": "0, 10, 0",
                "right-pinkie-second": "0, 10, 0",
                "right-pinkie-third": "0, 10, 0",

                "left-wrist": "0, 10, 0",
                "left-thumb-first": "0, 10, 0",
                "left-thumb-second": "0, 10, 0",
                "left-thumb-third": "0, 10, 0",
                "left-index-first": "0, 10, 0",
                "left-index-second": "0, 10, 0",
                "left-index-third": "0, 10, 0",
                "left-middle-first": "0, 10, 0",
                "left-middle-second": "0, 10, 0",
                "left-middle-third": "0, 10, 0",
                "left-ring-first": "0, 10, 0",
                "left-ring-second": "0, 10, 0",
                "left-ring-third": "0, 10, 0",
                "left-pinkie-first": "0, 10, 0",
                "left-pinkie-second": "0, 10, 0",
                "left-pinkie-third": "0, 10, 0"                
            },
            "body" : {
                "right-shoulder": "0, 10, 0",
                "left-shoulder": "0, 10, 0",
                "right-elbow": "0, 10, 0",
                "left-elbow": "0, 10, 0",
                "right-hip": "0, 10, 0",
                "left-hip": "0, 10, 0"
            }
        }
    };     
    res.send(JSON.stringify(testRoom));
});

module.exports = router;
