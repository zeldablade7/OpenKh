# EPD Format
EPD probably stands for Entity Parameter Data and contains all the stats related to the character such as HP or damage dealt by every move.

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | string   | File identifier, always `@EPD`
| 04     | int   | Version, `9`
