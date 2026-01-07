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
            padding="24,6"
            margin="6">
            <expander layout="stretch content"
              margin="0,0,0,4"
              is-expanded={<>IsExpanded}>
              <lane *outlet="header" *switch={:IsLoaded} vertical-content-alignment="middle" orientation="horizontal">
                <lane orientation="vertical" layout="stretch content" >
                  <label font="dialogue" max-lines="1" margin="4" text={PackName} />
                  <label font="small" max-lines="1" text={PackUniqueID} />
                </lane>
                <image *case="true" sprite={@mushymato.StardewModSmithy/sprites/emojis:checkmark} tooltip={#gui.tooltip.is-loaded} layout="36px 36px" margin="0,12,24,12"/>
                <image *case="false" sprite={@mushymato.StardewModSmithy/sprites/emojis:cross} tooltip={#gui.tooltip.not-loaded} layout="36px 36px" margin="12,12,24,12"/>
                <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                  layout="content[200..] content"
                  margin="0,0,8,0"
                  font="small"
                  text={#gui.button.edit-wallfloor}
                  opacity="0.5"
                  tooltip={#gui.tooltip.no-wallfloor-tx}
                  +state:canshow={:CanShowEdit_WallFloor}
                  +state:canshow:opacity="1.0"
                  +state:canshow:tooltip=""
                  left-click=|ShowEdit_WallFloor()|
                />
                <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                  layout="content[200..] content"
                  font="small"
                  text={#gui.button.edit-furniture}
                  opacity="0.5"
                  +state:canshow={:CanShowEdit_Furniture}
                  +state:canshow:opacity="1.0"
                  left-click=|ShowEdit_Furniture()|
                />
              </lane>
              <lane orientation="vertical" *if={IsExpanded}>
                <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
                <form-row-small title={#gui.label.author}>
                  <textinput font="small" layout="300px 54px" text={<>PackAuthor} />
                  <label text={#gui.label.mod-name} margin="32,8,8,8" font="small"/>
                  <textinput font="small" layout="stretch 54px" text={<>PackName} />
                </form-row-small>
                <form-row-small title={#gui.label.mod-desc}>
                  <textinput font="small" layout="stretch 54px" text={<>PackDescription} />
                </form-row-small>
                <form-row-small title={#gui.label.version}>
                  <textinput font="small" layout="200px 54px" text={<>PackVersion} />
                  <label text={#gui.label.nexus-id} margin="32,8,8,8" font="small"/>
                  <textinput font="small" layout="200px 54px" text={<>PackNexusID} />
                </form-row-small>
              </lane>
            </expander>
          </frame>
          <frame
            background={@Mods/StardewUI/Sprites/MenuSlotInset}
            layout="content[500..] 96px"
            padding="24,6"
            margin="6"
            vertical-content-alignment="middle">
            <lane orientation="horizontal" vertical-content-alignment="middle">
              <label font="dialogue" text={#gui.label.new-pack} />
              <textinput font="dialogue" layout="stretch 70px" margin="16,4,16,0" text={<>NewModName} />
              <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
                  layout="content[150..] content"
                  font="dialogue"
                  text={#gui.button.create}
                  left-click=|CreateAndEdit()|
                  opacity={NewModErrorOpacity}
                  tooltip={NewModTooltip}
                />
            </lane>
          </frame>
        </lane>
      </scrollable>
    </lane>
    <!-- Config -->
    <lane *case="config" *context={:ModConfig} orientation="vertical">
      <banner margin="8" text={#gui.label.mod-configuration} layout="content content"/>
      <scrollable peeking="128" scrollbar-margin="8,0,0,0">
        <lane orientation="vertical">
          <form-row title={#gui.label.show-workspace-key} tooltip={#gui.tooltip.show-workspace-key}>
            <keybind-editor button-height="64"
                sprite-map={@Mods/StardewUI/SpriteMaps/Buttons:default-default-0.5}
                editable-type="MultipleKeybinds"
                add-button-text={#gui.label.show-workspace-key.add}
                focusable="true"
                keybind-list={<>ShowWorkspaceKey} />
          </form-row>
          <form-row title={#gui.label.author} tooltip={#gui.tooltip.author-name}>
            <textinput layout="50% 64px" margin="-8,0" font="dialogue" placeholder={#gui.placeholder.author-name} text={<>AuthorName} />
          </form-row>
          <form-row title={#gui.label.autosave} tooltip={#gui.tooltip.autosave-frequency}>
            <frame *context={:AutosaveFrequency}
                background={@Mods/StardewUI/Sprites/MenuSlotOutset}
                padding="4">
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
      </scrollable>
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
    <label layout="300px content"
            font="dialogue"
            text={&title}
            tooltip={&tooltip}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>

<template name="form-row-small">
  <lane layout="content content"
        vertical-content-alignment="middle"
        margin="8">
    <label layout="content[150..] content"
            font="small"
            text={&title}/>
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
