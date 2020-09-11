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
| 04     | byte  | 
| 05     | byte  | 
| 06     | byte  | Guard State (O == 0x81) (X == 0x9) (/\ == 0x2)
| 07     | byte  | unknown `Usually always 0x64`
