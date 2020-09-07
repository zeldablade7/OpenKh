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
| 04     | float  | Max HP
| 08     | unk  | 
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
| 00     | string[]  | Animation List

### Other Parameters

| Offset | Type  | Description
|--------|-------|------------
| 00     | short  | Damage Ceiling
