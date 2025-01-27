# Refactor suggestions from Yoraiz
- [ ] Remove ProjectSceneManager (have individual games handle scene management)
- [ ] Have virtual SetupSession/CleanupSession methods in GameManager that game specific managers can override
- [ ] Make exit exiting lobbies close the lobby if you are the host

## Going back to main menu
- needs to be separate from main setup/cleanup session

## exit game scenario
- clicked resign button
- clicked exit game button (ends game session)
1. Notify other clients you left
2. exit the lobby (if host close the lobby)
3. request to close the session (boiler plate code)
4. Raise SessionCleanedUpEvent with the reason
5. Listener on MainMenuUI can respond to event and based on reason show appropriate UI response