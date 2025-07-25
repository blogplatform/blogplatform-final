from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager
import socketio

from app.db.database import connect_to_mongo, close_mongo_connection
from app.core.config import settings
from app.core.socket_manager import sio, socket_manager

# Import API routers
from app.routers.auth import router as auth_router
from app.routers.blogs import router as blogs_router
from app.routers.comments import router as comments_router
from app.routers.likes import router as likes_router
from app.routers.tags import router as tags_router
from app.routers.interests import router as interests_router
from app.routers.images import router as images_router
from app.routers.summaries import router as summaries_router
from app.routers.dashboard import router as dashboards_router


@asynccontextmanager
async def lifespan(app: FastAPI):
    await connect_to_mongo()
    yield
    await close_mongo_connection()


app = FastAPI(
    title="Blog Platform API",
    description="A comprehensive blogging platform with user authentication, blog management, comments, likes, and tags.",
    version="1.0.0",
    lifespan=lifespan
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:4200"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Register Routers
api_prefix = "/api/v1"
app.include_router(auth_router, prefix=api_prefix)
app.include_router(blogs_router, prefix=api_prefix)
app.include_router(images_router, prefix=api_prefix)
app.include_router(comments_router, prefix=api_prefix)
app.include_router(likes_router, prefix=api_prefix)
app.include_router(tags_router, prefix=api_prefix)
app.include_router(interests_router, prefix=api_prefix)
app.include_router(summaries_router, prefix=api_prefix)
app.include_router(dashboards_router, prefix=api_prefix)

# Mount Socket.IO
app.mount("/socket.io", socketio.ASGIApp(sio))

# Root and Health Endpoints
@app.get("/", tags=["Root"])
async def root():
    return {"message": "Hello from FastAPI!"}

@app.get("/health", tags=["Health"])    
async def health_check():
    return {"status": "healthy"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)