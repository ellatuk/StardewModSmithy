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
            layout="content[500..] content"
            padding="32,4"
            margin="8">
            <expander layout="stretch content"
              margin="0,0,0,4"
              header-padding="0,12"
              header-background-tint="#99c"
              is-expanded={<>IsExpanded}>
              <lane *outlet="header" orientation="horizontal" vertical-content-alignment="middle">
                <label layout="stretch content" font="dialogue" text={:PackTitle} />
                <image *if={:IsLoaded} sprite={@mushymato.StardewModSmithy/sprites/emojis:checkmark} layout="36px 36px" margin="12"/>
                <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                  layout="content[150..] content"
                  font="dialogue"
                  text={#gui.button.edit}
                  left-click=|ShowEditingMenu()|
                />
              </lane>
              <lane *if={IsExpanded} layout="content content"
                    vertical-content-alignment="middle"
                    margin="8">
                <label text={#gui.label.nexus-id}
                  margin="8" font="small"
                  shadow-alpha="0.8"
                  shadow-color="#4448"
                  shadow-offset="-2, 2" />
                <textinput  layout="300px 54px" text={<>NexusID} />
              </lane>
            </expander>
          </frame>
          <frame
            background={@Mods/StardewUI/Sprites/MenuSlotInset}
            layout="content[500..] 96px"
            margin="8"
            padding="32,4"
            vertical-content-alignment="middle">
            <lane orientation="horizontal" vertical-content-alignment="middle">
              <label font="dialogue" text={#gui.label.new-pack} />
              <textinput font="dialogue" layout="stretch 70px" margin="16,4,32,0" text={<>NewModName} />
              <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                  layout="content[150..] content"
                  font="dialogue"
                  text={#gui.button.create}
                  left-click=|CreateAndEdit()|
                  opacity={NewModErrorOpacity}
                  tooltip={NewModErrorMessage}
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
      <form-row title={#gui.label.auto-symlink-reload} tooltip={#gui.tooltip.auto-symlink-reload}>
        <checkbox is-checked={<>AutoSymlinkAndPatchReload}/>
      </form-row>
    </lane>
    <!-- About -->
    <lane *case="about" *context={:ModConfig} orientation="vertical">
      <banner margin="8" text={#gui.label.about} layout="content content"/>
      <about-label text={#gui.paragraph.about.0} />
      <about-label text={#gui.paragraph.about.1} />
      <about-label text={#gui.paragraph.about.2} />
      <about-label text={#gui.paragraph.about.3} />
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

<template name="about-label">
  <label text={&text}
    font="dialogue"
    margin="8"
    shadow-alpha="0.8"
    shadow-color="#4448"
    shadow-offset="-2, 2" />
</template>
