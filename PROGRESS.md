# Progress / Devlogs

## Timing Goals
I want a published product by September of 2026. I plan to not only make the game, but also market the game on Youtube, Instagram, Twitter, and TikTok to improve my odds of the game working.
<br></br>
To this end, I need to make sure I run two releases of the game, the alpha and the beta. The alpha will be a friends testing time. Ideally launched in the last two weeks of July. This should give give me an idea of major bugs and crashes the game experiences.
<br></br>
I am aware that this is an short timeline for the development of a game. If this becomes too short of a development timeline, then I will extend the final release to the end of the year and hope the alpha release will be available by the end of the summer instead.

## Day-to-Day Logs
### 05/05/2026
The first offical day working on this project. I have ported my movement from an old project. Next time I want to fix the aiming camera and tweak values for the movement to get the right feel.

### 05/08/2026
A few days into the project and I have a character that can move around with sprinting and crouching speeds. The aiming camera is good enough for now, but will likely need to be tweaked once I begin working on the combat system. Had to change the model a lot to get rid of weird scaling issues for animation and gave the arms/legs two joints instead of one. I also began the process of ragdoll physics on death. Only about 30 minutes of effort into it so far and it appears to be possessed. Progress is steady.

### 05/09/2026
Ragdoll added. Has to be one of the funniest things I have ever added to a game in unity. Once I make the player able to get back up, i'll figure out picking up items, then begin on animations.

### 05/15/2026
Changed the player model due to a reccomendation by my cousin. Definitely looks better but damn was that hard to make. Ragdoll logic was also smoothed out and the scripts are now interacting more and more. Getting complicated, but I keep making sure things stay on track. Next is the damage from falling and then picking up items.

### 05/22/2026
After a week vacation, not much has changed. I was able to make the player be reposistioned where the ragdoll hip would be and be angled properly. This week I attempted to also do a standup animation. The idea was there, but too many building blocks were missing and I endedup having a lot of patchwork code instead of actual working code. So I deleted it all, animations included, and revisted the foundation. Made the model have a hip that was originall missing and made the player have a handler. This handler will handle major state transitions. Within each of the scripts is more states that will all help me maintain the current state of the player. The next thing I want to do is setup a getting up animation. Once that is done, I can continue on what I had previously set out as my next goals.
