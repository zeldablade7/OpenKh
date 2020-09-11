# EPD Format
EPD probably stands for Entity Parameter Data and contains all the stats related to the character such as HP or damage dealt by every move.

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | string   | File identifier, always `@EPD`
| 04     | int   | Version, `9`

### General Parameters

| Offset | Type  | Description
|--------|-------|------------
| 00     | byte  | 
| 01     | byte  | 
| 02     | byte  | 
| 03     | byte  | 
| 04     | float | Max HP
| 08     | float | Experience Multiplier
| 0C     | unk  | 
| 10     | float  | Physical Damage Multiplier
| 14     | float  | Fire Damage Multiplier
| 18     | float  | Ice Damage Multiplier
| 1C     | float  | Thunder Damage Multiplier
| 20     | float  | Darkness Damage Multiplier
| 24     | float  | Non-Elemental Damage Multiplier

### Animation List

| Offset | Type  | Description
|--------|-------|------------
| 00     | string[24]  | Animation List

### Other Parameters

| Offset | Type  | Description
|--------|-------|------------
| 00     | short  | Damage Ceiling
| 02     | short  | Damage Floor?
| 04     | float  | 
| 08     | int    | 
| 0C     | int    | 
| 10     | int    | 
| 14     | int    | 
| 18     | int    | 
| 1C     | int    | 
| 20     | int    | 
| 24     | int    | 

### Animation Parameters

This structures repeats for as many animations need their parameters set.

| Offset | Type  | Description
|--------|-------|------------
| 00     | float  | Hit Damage Multiplier
| 04     | byte  | Animation index link?
| 05     | byte  | [Attack Kind](####Attack Kind)
| 06     | byte  | Guard State (O == 0x81) (X == 0x9) (/\ == 0x2)
| 07     | byte  | unknown `Usually always 0x64`

#### Attack Kind
| Id | Ailment |
|----|-------|
| 0x01 | Small Damage
| 0x02 | Big Damage
| 0x03 | Blow Damage
| 0x04 | Toss Damage
| 0x05 | Beat Damage
| 0x06 | Flick Damage
| 0x07 | Poison
| 0x08 | Slow
| 0x09 | Stop
| 0x0A | Bind
| 0x0B | Stun
| 0x0C | Freeze
| 0x0D | Burn
| 0x0E | Confu
| 0x0F | Blind
| 0x10 | Death
| 0x11 | Kill
| 0x12 | Capture
| 0x13 | Magnet
| 0x14 | Zero Gravity
| 0x15 | Aero (Fly in circles)
| 0x16 | Tornado (Violent version of Aero)
| 0x17 | Degenerator
| 0x18 | Without
| 0x19 | Eat
| 0x1A | Treasure Raid
| 0x1B | Sleeping Death
| 0x1C | Sleep
| 0x1D | Magnet Munny
| 0x1E | Magnet HP
| 0x1F | Magnet Focus
| 0x20 | Mini
| 0x21 | Quake
| 0x22 | Recover
| 0x23 | Discommand (Enemy attack won'tmake you budge)
| 0x24 | Disprize_M
| 0x25 | Disprize_H
| 0x26 | Disprize_F
| 0x27 | Detone
| 0x28 | GM_BLOW
| 0x29 | Blast
| 0x2A | Magnet Spiral
| 0x2B | Glacial Arts
| 0x2C | Transcendence
| 0x2D | Vengeance
| 0x2E | Magnet Breaker
| 0x2F | Magic Impulse CF
| 0x30 | Magic Impulse CFB
| 0x31 | Magic Impulse CFBB
| 0x32 | Rise Damage
| 0x33 | Stumble
| 0x34 | Mount
| 0x35 | Imprisonment (Character position = Enemy position)
| 0x36 | Slow Stop (Can result in Slow or Stop)
| 0x37 | Gathering (Freezes character for a long time)
| 0x38 | Exhausted (1HP, No Focus, No D-Link, All Commands in cooldown)
