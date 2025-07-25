from datetime import datetime, timedelta
from fastapi import APIRouter, Query
from app.db.database import get_database

router = APIRouter(prefix="/dashboard", tags=["dashboard"])

@router.get("/totals")
async def get_totals():
    db = await get_database()
    total_posts = await db.blogs.count_documents({})
    total_users = await db.users.count_documents({})
    total_likes = await db.likes.count_documents({})
    total_comments = await db.comments.count_documents({})

    return {
        "total_posts": total_posts,
        "total_users": total_users,
        "total_likes":total_likes,
        "total_comments": total_comments
    }

@router.get("/posts-over-time")
async def posts_over_time(range: str = Query("all", enum=["7d", "6m", "1y", "all"])):
    db = await get_database()
    today = datetime.utcnow()

    if range == "7d":
        start_date = today - timedelta(days=7)
        group_format = "%Y-%m-%d"
        group_by = "day"
    elif range == "6m":
        start_date = today - timedelta(days=30 * 6)
        group_format = "%Y-%m"
        group_by = "month"
    elif range == "1y":
        start_date = today - timedelta(days=365)
        group_format = "%Y-%m"
        group_by = "month"
    else:  # "all"
        first_blog = await db.blogs.find_one(sort=[("created_at", 1)])
        last_blog = await db.blogs.find_one(sort=[("created_at", -1)])
        if not first_blog or not last_blog:
            return {"labels": [], "counts": [], "group_by": "none"}
        start_date = first_blog["created_at"]
        total_days = (last_blog["created_at"] - start_date).days
        if total_days < 31:
            group_format = "%Y-%m-%d"
            group_by = "day"
        elif total_days < 365:
            group_format = "%Y-%m"
            group_by = "month"
        else:
            group_format = "%Y"
            group_by = "year"

    pipeline = [
        {"$match": {"created_at": {"$gte": start_date}}},
        {"$group": {
            "_id": {
                "$dateToString": {
                    "format": group_format,
                    "date": "$created_at"
                }
            },
            "count": {"$sum": 1}
        }},
        {"$sort": {"_id": 1}}
    ]

    results = await db.blogs.aggregate(pipeline).to_list(None)

    labels = [item["_id"] for item in results]
    counts = [item["count"] for item in results]

    return {"labels": labels, "counts": counts, "group_by": group_by}
  
@router.get("/posts-by-category")
async def posts_by_category():
    db = await get_database()
    pipeline = [
        {"$group": {"_id": "$tags", "count": {"$sum": 1}}},
    ]
    result = await db.blogs.aggregate(pipeline).to_list(length=10)

    # Convert tag list to string or flat map if needed
    formatted = []
    for item in result:
        if isinstance(item["_id"], list):
            for tag in item["_id"]:
                formatted.append({"name": tag, "count": item["count"]})
        else:
            formatted.append({"name": item["_id"], "count": item["count"]})

    return formatted

@router.get("/top-tags")
async def top_tags():
    db = await get_database()
    pipeline = [
        {"$unwind": "$tags"},
        {"$group": {"_id": "$tags", "count": {"$sum": 1}}},
        {"$sort": {"count": -1}},
        {"$limit": 20}
    ]
    result = await db.blogs.aggregate(pipeline).to_list(length=20)
    return [{"name": r["_id"], "value": r["count"]} for r in result]
  
@router.get("/most-liked")
async def most_liked():
    db = await get_database()
    cursor = db.blogs.find().sort("likes_count", -1).limit(5)
    result = await cursor.to_list(length=5)
    return [{"title": r["title"], "likes": r["likes_count"]} for r in result]
  
@router.get("/users-over-time")
async def users_over_time(range: str = Query("all", enum=["7d", "6m", "1y", "all"])):
    db = await get_database()
    today = datetime.utcnow()

    if range == "7d":
        start_date = today - timedelta(days=7)
        group_format = "%Y-%m-%d"
        group_by = "day"
    elif range == "6m":
        start_date = today - timedelta(days=30 * 6)
        group_format = "%Y-%m"
        group_by = "month"
    elif range == "1y":
        start_date = today - timedelta(days=365)
        group_format = "%Y-%m"
        group_by = "month"
    else:
        first_user = await db.users.find_one(sort=[("created_at", 1)])
        last_user = await db.users.find_one(sort=[("created_at", -1)])
        if not first_user or not last_user:
            return {"labels": [], "counts": [], "group_by": "none"}
        start_date = first_user["created_at"]
        total_days = (last_user["created_at"] - start_date).days
        if total_days < 31:
            group_format = "%Y-%m-%d"
            group_by = "day"
        elif total_days < 365:
            group_format = "%Y-%m"
            group_by = "month"
        else:
            group_format = "%Y"
            group_by = "year"

    pipeline = [
        {"$match": {"created_at": {"$gte": start_date}}},
        {"$group": {
            "_id": {
                "$dateToString": {
                    "format": group_format,
                    "date": "$created_at"
                }
            },
            "count": {"$sum": 1}
        }},
        {"$sort": {"_id": 1}}
    ]

    result = await db.users.aggregate(pipeline).to_list(None)

    return {
        "labels": [r["_id"] for r in result],
        "counts": [r["count"] for r in result],
        "group_by": group_by
    }