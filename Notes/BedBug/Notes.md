

GameManager.cs Line 2810 
```
	public void ChangeBlocks(string persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
	if (block is BlockSleepingBag || block2 is BlockSleepingBag)
	{
		EntityAlive entityAlive = entity as EntityAlive;
		if (entityAlive)
		{
			if (block is BlockSleepingBag)
			{
				NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(entityAlive, "sleeping_bag");
				entityAlive.SpawnPoints.Set(blockChangeInfo.pos);
			}
			else
			{
				this.persistentPlayers.SpawnPointRemoved(blockChangeInfo.pos);
			}
			flag = true;
		}
	}
```