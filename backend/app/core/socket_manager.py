import socketio
from typing import Optional
import logging
from motor.motor_asyncio import AsyncIOMotorClient

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Create Socket.IO server with CORS settings
sio = socketio.AsyncServer(
    async_mode='asgi',
    cors_allowed_origins=["http://localhost:4200"],
    logger=True,
    engineio_logger=True
)

class SocketManager:
    def __init__(self):
        self.connected_clients = set()
    
    async def connect_client(self, sid: str):
        """Add client to connected set"""
        self.connected_clients.add(sid)
        logger.info(f"Client {sid} connected. Total clients: {len(self.connected_clients)}")
    
    async def disconnect_client(self, sid: str):
        """Remove client from connected set"""
        self.connected_clients.discard(sid)
        logger.info(f"Client {sid} disconnected. Total clients: {len(self.connected_clients)}")
    
    async def emit_dashboard_update(self, data_type: str, data: dict):
        """Emit dashboard data updates to all connected clients"""
        if self.connected_clients:
            await sio.emit('dashboard_update', {
                'type': data_type,
                'data': data
            })
            logger.info(f"Emitted {data_type} update to {len(self.connected_clients)} clients")
    
    async def emit_blog_update(self, action: str, blog_data: dict):
        """Emit blog-related updates"""
        if self.connected_clients:
            await sio.emit('blog_update', {
                'action': action,  # 'created', 'updated', 'deleted'
                'data': blog_data
            })
            logger.info(f"Emitted blog {action} update to {len(self.connected_clients)} clients")
    
    async def emit_user_update(self, action: str, user_data: dict):
        """Emit user-related updates"""
        if self.connected_clients:
            await sio.emit('user_update', {
                'action': action,  # 'registered', 'updated', 'deleted'
                'data': user_data
            })
            logger.info(f"Emitted user {action} update to {len(self.connected_clients)} clients")
    
    async def emit_like_update(self, action: str, like_data: dict):
        """Emit like-related updates"""
        if self.connected_clients:
            await sio.emit('like_update', {
                'action': action,  # 'added', 'removed'
                'data': like_data
            })
            logger.info(f"Emitted like {action} update to {len(self.connected_clients)} clients")
    
    async def emit_comment_update(self, action: str, comment_data: dict):
        """Emit comment-related updates"""
        if self.connected_clients:
            await sio.emit('comment_update', {
                'action': action,  # 'created', 'updated', 'deleted'
                'data': comment_data
            })
            logger.info(f"Emitted comment {action} update to {len(self.connected_clients)} clients")

# Create global socket manager instance
socket_manager = SocketManager()

# Socket.IO event handlers
@sio.event
async def connect(sid, environ):
    """Handle client connection"""
    await socket_manager.connect_client(sid)
    await sio.emit('connection_confirmed', {'message': 'Connected to blog platform'}, room=sid)

@sio.event
async def disconnect(sid):
    """Handle client disconnection"""
    await socket_manager.disconnect_client(sid)

@sio.event
async def join_dashboard(sid):
    """Allow clients to join dashboard updates"""
    await sio.enter_room(sid, 'dashboard')
    logger.info(f"Client {sid} joined dashboard room")

@sio.event
async def leave_dashboard(sid):
    """Allow clients to leave dashboard updates"""
    await sio.leave_room(sid, 'dashboard')
    logger.info(f"Client {sid} left dashboard room")

# Create the ASGI app for Socket.IO
socket_app = socketio.ASGIApp(sio)

