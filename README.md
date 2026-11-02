# TAPESBOUND
A project to demonstrate the possibilities of creating a video game. This game was made to be showcased for an audience on the Ferstruc TAPES Vol 2. Gameplay and art are inspired by the SNES classic Earthbound.

Open the project with **Unity 6.3 LTS (6000.3.27f2)**.

## Dialogue & quests

Everything below is set up in the Inspector, no code needed. The City scene has a working example: Shoenice's liquor quest.

### Writing dialogue
Create one through `Create > Dialogue`:

| Asset | Use |
| --- | --- |
| Dialogue Text | Plain lines. |
| Dialogue Event | Lines + UnityEvents that fire on a line number. |
| Dialogue Options | Lines + up to 2 choices (Yes/No) shown after the last line. |
| Dialogue Timeline | Pauses the timeline while it's on screen. |

Every line has a **Commands** list that runs when the line appears:

| Command | Does |
| --- | --- |
| GiveItem / TakeItem | Adds/removes an item and prints *"You got the Liquor!"* right after the line. |
| GiveMoney / TakeMoney | Same for money. |
| StartQuest / CompleteQuest | Starts or completes a quest (rewards included). |
| AdvanceQuest | Adds progress to an objective (by key, empty = current objective). |
| SetFlag / ClearFlag | Story flags, like Earthbound's event flags. |
| PlayDialogue / PlaySound | Plays another dialogue right after this line / a sound. |

Tick **Silent** to skip the message. Text can use `{player}` and `{money}`, and rich text like `<color=red>` types out correctly.
Each option of a Dialogue Options has a Next Dialogue, Commands and a UnityEvent. An option without a Next Dialogue ends the conversation.
The messages ("You got the {item}!") and an optional text blip sound and `@` line prefix are set on the **DialogueManager**.

### NPCs
`DialogueTrigger` plays its **Default Dialogue** (cycling each talk), unless one of the **Conditional Dialogue** entries matches. They are checked top to bottom and the first one where all conditions pass is used:

- **QuestState**: a quest is NotStarted / Active / ReadyToTurnIn / Completed
- **Flag**: a story flag is set
- **HasItem / HasMoney**

Every condition has a **Not** toggle. Put the most specific situations at the top, e.g. Shoenice: *Completed → Ready to turn in → Active → Not started*.
A DialogueTrigger also works without an NPC component (signs, presents, trash cans).

### Quests
Create one through `Create > Quest > Quest`:

- **Objectives**: `TalkTo` an NPC (drag the NPC from the open scene into *NPC In Scene*), `CollectItem`, `Flag` (e.g. set by an EventTriggerZone when reaching a place) or `Custom` (advanced by an AdvanceQuest command with the same key).
- **Objectives In Order**: steps are done one by one, later steps stay hidden in the quest log.
- **Requires Turn In**: when all objectives are done the quest waits as *Ready to turn in* until a CompleteQuest command runs, usually in the quest giver's conditional dialogue. Without it the quest completes by itself.
- **Rewards**: money and items are given on completion and printed in the text box. *On Complete Commands* can set flags or start the next quest. *Complete Dialogue* replaces the default "Quest complete" message.

Start a quest with a StartQuest command, or add it to **Starting Quests** on the scene's QuestManager.
Progress is kept by the QuestManager (it survives loading another scene), never inside the quest asset.

### Quest log
Press **Q** to open the quest log: up/down to select, **Space** to track a quest, **Q** or **Shift** to close.
The tracker in the top left shows the current step of the tracked quest. The QuestManager creates this UI when the scene has none, copying the dialogue box's font and window. Add a `QuestUI` component yourself to change its style or text.

### Events
- **EventTriggerZone**: put it on a trigger collider. When the player walks in it plays a dialogue, runs commands and/or a UnityEvent. Use *Once Flag* to only trigger it once.
- The **GenericEvents** asset (`EventBehaviour`) has StartQuest, CompleteQuest, SetFlag, GiveMoney, JoinParty and LeaveParty for UnityEvents and timeline signals.

## Party & co-op

### Party members
A party member is a **Character** (`Create > Character > New Character`): a name and an animator controller with `Horizontal`, `Vertical` and `IsWalking` (`LastHorizontal`/`LastVertical` are optional).

- **Joining**: use the **JoinParty** / **LeaveParty** commands (on a dialogue line, option, quest or event zone). They print *"Paula joined the party!"* unless Silent. Or add the character to **Starting Members** on the scene's PartyManager.
- Like Earthbound, up to 3 members travel with the player. They walk in line behind the player (**Line Spacing**), and the party stays the same when loading another scene.
- Every scene with a player gets a PartyManager by itself. Add one to the Player yourself to change its settings.
- **What a member looks like**: a `Follower` placed in the scene for that character is used (it stays hidden until they join, like *Peniswise Party* in City). Otherwise the PartyManager creates one, from **Member Prefab** when set.
- The **InParty** condition works everywhere conditions do. Put a **ConditionalActive** component on an NPC with *InParty (Not)* to make them disappear once they joined. It also works for other things, like showing someone after a quest.
- City has Peniswise as a starting member to try it out. Remove them from the Player's PartyManager to undo that.

### Playing together
Player 1 plays the leader with the keyboard and the first controller they use. Anyone else presses **Start** on their own controller to take over the next free party member (P2 to P4), and **Select** to give it back; it then walks back into line. Unplugging a controller also leaves.

| | Keyboard | Gamepad | Other joysticks |
| --- | --- | --- | --- |
| Walk | Arrows / WASD | Left stick / D-pad | Stick / hat |
| Talk, check, read on | Space | A | Button 1 or 2 |
| Menu | Shift | B | Button 3 |
| Quest log | Q | Y | Button 4 |
| Join as a party member | | Start | Button 10 |
| Leave | | Select / Back | Button 9 |

- Everybody stays on one screen: the camera frames all players and nobody can walk off the screen (**Screen Margin** on the PartyManager).
- Co-op players can talk to NPCs, check things, read on in the text box and trigger EventTriggerZones. The menu and quest log belong to player 1. Everybody waits while player 1 is in a conversation, a menu or a cutscene.
- "P1"/"P2" tags show above the characters and join messages show top right. The PartyManager creates this `CoopUI`; add one yourself to change the colors or text.
