<lane orientation="horizontal"
  horizontal-content-alignment="middle"
  vertical-content-alignment="start">
  <lane layout="120px content"
        margin="0, 32, 0, 0"
        orientation="vertical"
        horizontal-content-alignment="end"
        z-index="2">
    <frame *repeat={:AllTabs}
        layout="100px 64px"
        margin={Margin}
        padding="16, 0"
        horizontal-content-alignment="middle"
        vertical-content-alignment="middle"
        background={@mushymato.StardewModSmithy/sprites/MenuTiles:TabButton}
        focusable="true"
        click=|^SelectTab(this)|>
      <label text={:Label} />
    </frame>
  </lane>
  <frame *switch={SelectedTab}
      background={@Mods/StardewUI/Sprites/MenuBackground}
      border={@Mods/StardewUI/Sprites/MenuBorder}
      border-thickness="36, 36, 40, 36"
      margin="-20,0,0,0"
      padding="8"
      layout="70%[1000..] 80%[650..]">
    <!-- Packs -->
    <lane *case="packs" *context={:PackListing} orientation="vertical">
      <banner margin="8" text={#gui.label.editable-content-packs} layout="content content"/>
      <scrollable peeking="128" scrollbar-margin="8,0,0,0">
        <lane orientation="vertical">
          <frame *repeat={PackDisplayList}
            background={@Mods/StardewUI/Sprites/MenuSlotInset}
            layout="content[500..] 96px"
            padding="32,0"
            margin="8"
            focusable="true"
            left-click=|ShowEditingMenu()|>
            <panel layout="stretch stretch" vertical-content-alignment="middle">
              <label font="dialogue" text={:PackTitle}/>
            </panel>
          </frame>
          <frame
            background={@Mods/StardewUI/Sprites/MenuSlotInset}
            layout="content[500..] 96px"
            margin="8"
            vertical-content-alignment="middle">
            <lane orientation="horizontal" horizontal-content-alignment="end" vertical-content-alignment="middle">
              <label font="dialogue" text={#gui.label.new-pack} margin="32,0,0,0" />
              <textinput layout="stretch 54px" margin="16,4,32,0" text={<>NewModName} />
              <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
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
    <!-- Config -->
    <lane *case="config" *context={:ModConfig} orientation="vertical">
      <banner margin="8" text={#gui.label.mod-configuration} layout="content content"/>
      <form-row title={#gui.label.author} tooltip={#gui.tooltip.author-name}>
        <textinput layout="stretch 64px" margin="16" font="dialogue" text={<>AuthorName} />
      </form-row>
      <form-row title={#gui.label.autosave} tooltip={#gui.tooltip.autosave-frequency}>
        <frame *context={:AutosaveFrequency}
            background={@Mods/StardewUI/Sprites/MenuSlotOutset}
            padding="4"
            margin="24,0,0,0">
          <segments balanced="true"
              highlight={@Mods/StardewUI/Sprites/White}
              highlight-tint="#39d"
              highlight-transition="150ms EaseOutQuart"
              separator={@Mods/StardewUI/Sprites/White}
              separator-tint="#c99"
              separator-width="2"
              selected-index={<>SelectedIndex}>
            <label *repeat={Segments}
              layout="content[200..] content"
              padding="16"
              text={:Name}
              tooltip={:Description} />
          </segments>
        </frame>
      </form-row>
    </lane>
  </frame>
</lane>

<template name="form-row">
  <lane layout="content content"
        vertical-content-alignment="middle"
        margin="16">
    <label layout="200px content"
            font="dialogue"
            text={&title}
            tooltip={&tooltip}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>

