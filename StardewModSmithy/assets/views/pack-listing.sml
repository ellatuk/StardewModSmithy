<frame background={@Mods/StardewUI/Sprites/MenuBackground}
       border={@Mods/StardewUI/Sprites/MenuBorder}
       border-thickness="36, 36, 40, 36"
       layout="70% 80%">
  <lane orientation="vertical">
    <banner margin="8" text={#gui.label.editable-content-packs} layout="content content"/>
    <scrollable peeking="128" scrollbar-margin="8,0,0,0">
      <lane orientation="vertical">
        <frame *repeat={PackDisplayList}
          background={@Mods/StardewUI/Sprites/MenuSlotInset}
          layout="content[500..] 80px"
          padding="32,0"
          margin="8"
          focusable="true"
          left-click=|ShowEditingMenu()|>
          <panel layout="stretch stretch" vertical-content-alignment="middle">
            <label font="small" text={:PackTitle}/>
          </panel>
        </frame>
        <frame
          background={@Mods/StardewUI/Sprites/MenuSlotInset}
          layout="content[500..] 80px"
          margin="8"
          vertical-content-alignment="middle">
          <lane orientation="horizontal" horizontal-content-alignment="end" vertical-content-alignment="middle">
            <label text={#gui.label.new-pack} margin="32,0,0,0" />
            <textinput layout="stretch 54px" margin="16,4,32,0" text={<>NewModName} />
            <button
                layout="content[130..] content"
                margin="0,0,36,0"
                text={#gui.button.create}
                left-click=|CreateAndEdit()|
              />
          </lane>
        </frame>
      </lane>
    </scrollable>
  </lane>
</frame>
