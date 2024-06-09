const { MongoClient } = require('mongodb');

const uri = 'mongodb://0.0.0.0:27017'; // Replace with your MongoDB connection string

let client, connection;

// General, entry database connection function, invoked upon server startup
async function connectToDatabase() {
  client = new MongoClient(uri);
  try {
    await client.connect();
    console.log('Connected to MongoDB');
    
    connection = getDb();
  
  } catch (error) {
    console.error('Failed to connect to MongoDB', error);
    throw error;
  }
}

// Called only in here, to retrieve the DB connection
function getDb() {
  if (!client) {
    throw new Error('You must connect first!');
  }
  return client.db('RealEngine'); // Replace with your database name
}

// Get a room itself by its name
async function findRoomByName(name) {
    var roomBufForm = await retrieveData({"roomID": name });
    return roomBufForm[0];
}

// Get all asset info from a specific room.
async function getAssetsByRoom(name) {
    let roomBlock = [];
    roomBlock = await findRoomByName(name).assets;
    return roomBlock;
}

// Get a user's info by their name, and their room
async function getUserByName(room, name) {
    let potentialUsers = await getUsersByRoom(room);
    for (const [user, data] of Object.entries(potentialUsers)) {
        if (potentialUsers[user].username === name)
            return potentialUsers[user];
    }
    throw new Error(`User in room ${room} with name ${name} not found.`);
}

// Get all user info from a specific room.
async function getUsersByRoom(name) {
    let userBlock = [];
    let first = await findRoomByName(name);
    userBlock = first[0].users;
    return userBlock;
}

// Create a room with a name and flags
async function createRoomWithName(name, flags) {
    let testRoom = {
            "roomID" : "VONK",
            "users" : {
                "Berkan" : {
                    "username" : "Berkan",
                    "id" : "Berkan",
                    "xp" : 0,
                    "hp": 100,
                    "kinematics" : {
                        "hand": {
                            "right-wrist": "0, 100, 0",
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
                }
            }
        };

    try {
        console.log("Trying...");
        const res = await connection.collection("rooms").insertOne(testRoom);
    }
    catch (e) {
        console.log(e);
    }
}

// Insert data into a room
async function insertData(room, data) {
    await connection.collection("rooms").updateOne({"roomID" : room}, data, async (err, res) => {
        if (err) throw err;
        console.log("Updated.");
        db.close();
    });
}

// General function for querying the DB, reduces redundancy
async function retrieveData(query) {
    let dataBuffer = [];
    if (connection === undefined) {
        throw new Error('DB has not been connected!');
    }
    const dbresult = await connection.collection("rooms").find(query);
    for await (const doc of dbresult) {
        dataBuffer.push(doc);
    }
    return dataBuffer;
}

async function insertSampleData() {
    createRoomWithName("VONK");
}

module.exports = { 
    connectToDatabase, 
    getUserByName,
    getAssetsByRoom,
    getUsersByRoom,
    findRoomByName,
    retrieveData,
    insertSampleData,
    createRoomWithName,
    insertData,
    connection
};
