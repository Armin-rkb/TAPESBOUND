# TAPESBOUND
A project to demonstrate the possibilities of creating a video game. This game was made to be showcased for an audience on the Ferstruc TAPES Vol 2. Gameplay and art are inspired by the SNES classic Earthbound.

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
- The **GenericEvents** asset (`EventBehaviour`) has StartQuest, CompleteQuest, SetFlag and GiveMoney for UnityEvents and timeline signals.
