/*
* index.js - Berkan Mertan
* This is the ENTRY FILE for the Node + Express + Mongo + SocketIO application in charge of the
* grand multiplayer server system. This code handles the server-client relationship between all
* players connected to the game and the user database. This code is also responsible for maintaining
* synchronization of socket communication with the players such that no data read/write errors occur,
* and all updates are accurately reflected on BOTH endpoints without hang ups, errors, or syncing issues.
*/

// Import needed dependencies
const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const routes = require('./routing');
const db = require('./db');
const fs = require('fs');
const { CONNREFUSED } = require('dns');

// Import express js
const app = express();
const server = http.createServer(app);
const io = socketIo(server);

// Eventually needed to prevent an error
let dbConnected = false;

// Middleware to handle JSON
app.use(express.json());

// Use routes defined in routes.js
app.use(routes);

// Async task used in socket connection callbacks for
// updating a user's database-stored info once they request their changes be made.
async function refreshDBWithUserBuffer (userBuffer) {
  await db.insertData(roomName, {
    $set: {
      "users" : {
        ...userBuffer
      }
    }
  });
}

// Code redundancy is present here...
// Socket.IO connection handling and database gateway
io.on('connection', (socket) => {
  console.log('A user connected');

  socket.on('message', (msg) => {
    console.log('Message received: ' + msg);
    socket.emit('message', 'Hello from server');
  });

  // Room based events
  socket.on('create-room', (name) => {
    console.log(`Creating room with name ${name}...`);
  });

  socket.on('disconnect', () => {
    console.log('User disconnected');
  });

  socket.on('join-room', async (roomName) => {
    socket.join(roomName);
    io.to(roomName).emit("send-general-signal", `Joined room ${roomName}...`)
  });

  
  // Only for AFTER a player hasbeen instantiated, and they are pinging their info
  // back to the server to update their own logs on the database.
  socket.on('ping-kinematic-info', async (roomName, userName, data) => {
    let roomData = await db.findRoomByName(roomName);
    let userBuffer = roomData["users"];
    let newUserData = JSON.parse(data);

    userBuffer[userName] = newUserData;
    await refreshDBWithUserBuffer(userBuffer);
    
  });

  // Only for adding a new user, BEFORE instantiation on the game end. See Unity script
  // for Multiplayer.cs to see full custom multiplayer pipeline for data transfer
  socket.on('add-new-user', async (roomName, userName, data) => {
    let roomData = await db.findRoomByName(roomName);
    let userBuffer = roomData["users"];
    let newUserData = JSON.parse(data);

    userBuffer = { ...userBuffer, [userName]: {...newUserData}};
    await refreshDBWithUserBuffer(userBuffer);
  });

  // Do not hold unnecessary data, delete a user's info when they quit the game
  socket.on('delete-user', async (roomName, userName) => {
    let roomData = await db.findRoomByName(roomName);
    let userBuffer = roomData["users"];
    delete userBuffer[userName];

    await refreshDBWithUserBuffer(userBuffer);
  });
});

// Ping players every 10ms with new data from the DB, so that every party is aware of 
// other players' actions, positions, etc.
setInterval(async () => {
  if (dbConnected)
    io.to("VONK").emit("refresh-data-from-db", await db.findRoomByName("VONK"));
}, 10);

// Connect to MongoDB
db.connectToDatabase().then(() => {
  const PORT = process.env.PORT || 3000;

  // Init server on port
  server.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);

    // Db connection established
    dbConnected = true;
  });
}).catch((error) => {
  console.error('Failed to connect to database:', error);
});