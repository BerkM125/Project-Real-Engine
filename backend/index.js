// This server will be in charge of facilitating multiplayer interaction through sockets.
// Also will be in charge of handling all data requests made by the unity client side.
// Sockets will send all game data from the server to EVERY client, and all changes shouldbe reflected that way.
// Server will be hosted on a separate device.
const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const routes = require('./routing');
const db = require('./db');
const fs = require('fs');
const { CONNREFUSED } = require('dns');

const app = express();
const server = http.createServer(app);
const io = socketIo(server);
let dbConnected = false;
// Middleware to handle JSON
app.use(express.json());

// Use routes defined in routes.js
app.use(routes);

// Socket.IO connection handling
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

  // Only for adding a new user
  socket.on('add-new-user', async (roomName, userName, data) => {
    let roomData = await db.findRoomByName(roomName);
    let userBuffer = roomData["users"];
    let newUserData = JSON.parse(data);

    userBuffer = { ...userBuffer, [userName]: {...newUserData}};
    await db.insertData(roomName, {
      $set: {
        "users" : {
          ...userBuffer
        }
      }
    });
  });

  socket.on('delete-user', async (roomName, userName) => {
    let roomData = await db.findRoomByName(roomName);
    let userBuffer = roomData["users"];
    delete userBuffer[userName];

    await db.insertData(roomName, {
      $set: {
        "users" : {
          ...userBuffer
        }
      }
    }); 
  });

  // Only for updating a specific users stuff
  socket.on('ping-kinematic-info', async (roomName, userName, data) => {
    let roomData = await db.findRoomByName(roomName);
    let userBuffer = roomData["users"];
    let newUserData = JSON.parse(data);

    userBuffer[userName] = newUserData;
    //console.log(newUserData);

    await db.insertData(roomName, {
      $set: {
        "users" : {
          ...userBuffer
        }
      }
    });
    
  });

});

// Ping these guys every half a second.
setInterval(async () => {
  if (dbConnected)
    io.to("VONK").emit("refresh-data-from-db", await db.findRoomByName("VONK"));
}, 10);

// Connect to MongoDB
db.connectToDatabase().then(() => {
  const PORT = process.env.PORT || 3000;
  server.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
    dbConnected = true;
  });
}).catch((error) => {
  console.error('Failed to connect to database:', error);
});

module.exports = io