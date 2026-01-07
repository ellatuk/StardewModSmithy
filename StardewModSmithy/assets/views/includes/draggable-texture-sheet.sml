<lane orientation="horizontal" layout="stretch stretch">
  <frame *if={ShowingTextureSelector}
      layout="content 100%" 
      background={@Mods/StardewUI/Sprites/ControlBorder}
      padding="16"
      z-index="2">
    <scrollable peeking="128" scrollbar-margin="8,0,0,0">
      <lane orientation="vertical">
        <frame *repeat={Textures}
          border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          border-thickness="4"
          background={@Mods/StardewUI/Sprites/White}
          background-tint="Transparent"
          focusable="true"
          padding="4"
          left-click=|^SelectTextureAsset(this)|
          right-click=|^SelectTextureAssetFront(this)|
          +transition:background-tint="100ms EaseOutCubic"
          +hover:background-tint="Wheat"
          +state:selected={IsSelected}
          +state:selected:background-tint="#39d"
          +state:selected-front={IsSelectedFront}
          +state:selected-front:background-tint="#3344DE"
          >
          <image layout="192px content[128..]" sprite={UISpriteSmall} fit="Contain" vertical-alignment="middle"/>
        </frame>
      </lane>
    </scrollable>
  </frame>
  <image
    sprite={@mushymato.StardewModSmithy/sprites/cursors2:paintButton}
    layout="64px 64px"
    margin="8,-4,8,8"
    focusable="true"
    +hover:scale="1.1"
    left-click=|ToggleTextureSelector()|
    z-index="2"/>

  <panel layout="stretch stretch"
    margin="0,64,0,0"
    draggable="true"
    drag-start=|SheetDragStart($Position)|
    drag=|SheetDrag($Position)|
    drag-end=|SheetDragEnd($Position)|>
    <image sprite={Sheet} margin={SheetMargin} opacity={SheetOpacity} />
    <image *if={HasSheetFront} sprite={SheetFront} margin={SheetMargin} opacity={SheetOpacity} />
    <panel layout="content content" padding={BoundsPadding} *context={BoundsProvider}>
      <image *repeat={GUI_BoundingSquares} margin={:this} sprite={@mushymato.StardewModSmithy/sprites/cursors:tileGreen} />
      <image sprite={@mushymato.StardewModSmithy/sprites/cursors:borderWhite}
        layout={GUI_TilesheetArea}
        fit="Stretch"
        opacity="0.5"/>
      <label text={BoundsLabel} font="dialogue" color="White" margin="12,2,0,0" scale="1.5" shadow-alpha="1" shadow-color="#4448" shadow-offset="-4, 4"/>
    </panel>
    <frame *if={CanDrag}
        *context={:MovementMode}
        background={@Mods/StardewUI/Sprites/MenuSlotOutset}
        margin="0,-64,0,0"
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
          margin="12, 8"
          text={:Name}
          tooltip={:Description} />
      </segments>
    </frame>
  </panel>
</lane>
