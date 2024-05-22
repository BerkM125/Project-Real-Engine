// This server will be in charge of facilitating multiplayer interaction through sockets.
// Also will be in charge of handling all data requests made by the unity client side.
// Sockets will send all game data from the server to EVERY client, and all changes shouldbe reflected that way.
// Server will be hosted on a separate device.
const express = require('express');
const http = require('http');
const socketIo = require('socket.io');
const routes = require('./routing');
const { connectToDatabase } = require('./db');

const app = express();
const server = http.createServer(app);
const io = socketIo(server);

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
});

// Connect to MongoDB
connectToDatabase().then(() => {
  const PORT = process.env.PORT || 3000;
  server.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
  });
}).catch((error) => {
  console.error('Failed to connect to database:', error);
});
