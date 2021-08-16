# Roles Overview Creator - v2

[TODO]


# Roles Overview Creator - v1

## How to use 
1) Open `role.html` in browser
    - (*optional*: get and install better font from https://www.fontget.com/font/vcr-osd-mono/)
2) Use Zoom (`Ctrl`+`-`) and Resize Element (**lower right corner of black background**) to adjust the overview to a good size
3) Take screenshot and download it
    - (`Ctrl`+`Shift`+`s`) and select wanted area
        - in Firefox: click on black background (where no role element is, mostly in lower right corner or leave a small place on the bottom when resizing)
    - **or** press `F12` and go to elements/inspector, right click on `<div class="overview">`, click on 'Node Screenshot'

## How to add roles
1) Open `role.html` in editor
2) go to `<body>`/`<script>`/`const roles = []`
3) add new object 
    ```javascript
        {
            name: "Role Name",
            desc: "Role Description with <#FF1919>Custom Colors</> for single words",
            color: "#FF1919", [HEX color value for Role Name]
        }, 
    ```

### Further Work (maybe)
- use HTML canvas element or other library, such that image can be downloaded in high quality without adjusting
- connect roles array, such that it doesn't have to be edited manually
