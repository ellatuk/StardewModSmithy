<panel layout="100% 100%" horizontal-content-alignment="end">
  <panel layout="stretch stretch" margin="0,0"
    draggable="true"
    drag-start=|SheetDragStart($Position)|
    drag=|SheetDrag($Position)|
    drag-end=|SheetDragEnd($Position)|>
    <image sprite={FurnitureSheet} margin={FurnitureSheetMargin} opacity={FurnitureSheetOpacity}/>
    <panel layout="content content" padding={SelectionBoundsPadding} *context={SelectedFurniture}>
      <image *repeat={GUI_BoundingSquares} margin={:this} sprite={@mushymato.StardewModSmithy/sprites/cursors:tileGreen} />
      <image sprite={@mushymato.StardewModSmithy/sprites/cursors:borderWhite}
        layout={GUI_TilesheetArea}
        fit="Stretch"
        opacity="0.5"/>
      <label text={SpriteIndex} font="dialogue" color="White" padding="8,4" scale="1.5" shadow-alpha="1" shadow-color="#4448" shadow-offset="-4, 4"/>
    </panel>
  </panel>
  <panel layout="500px content">
    <frame layout="stretch 96px" padding="30,20" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <lane orientation="vertical">
        <form-row title={#gui.label.id}>
          <dropdown options={FurnitureDataList}
            option-format={:FurnitureDataName}
            option-min-width="240"
            selected-option={<>SelectedFurniture}/>
        </form-row>
        <form-row title={#gui.label.dragging}>
          <enum-segments *context={:MovementMode} />
        </form-row>
      </lane>
    </frame>
    <frame layout="stretch stretch" margin="0,120,0,0" padding="32,8" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasSelectedFurniture}>
      <lane orientation="vertical" *context={SelectedFurniture}>
        <form-row title={#gui.label.name}>
          <textinput layout="content 54px" margin="-8,12" text={<>DisplayName} />
        </form-row>
        <form-row title={#gui.label.tilesheet}>
          <spin-box *context={TilesheetSizeX} />
          <spin-box *context={TilesheetSizeY} />
        </form-row>
        <form-row title={#gui.label.bounding}>
          <spin-box *context={BoundingBoxSizeX} />
          <spin-box *context={BoundingBoxSizeY} />
        </form-row>
        <form-row title={#gui.label.type}>
          <dropdown layout="stretch content"
            options={Type_Options}
            option-min-width="240"
            selected-option={<>Type} />
        </form-row>
        <form-row title={#gui.label.rotation}>
          <dropdown options={Rotation_Options}
            option-format={:RotationName}
            option-min-width="240"
            selected-option={<>Rotation}/>
        </form-row>
        <form-row title={#gui.label.placement}>
          <dropdown options={Placement_Options}
            option-format={:PlacementName}
            option-min-width="240"
            selected-option={<>Placement}/>
        </form-row>
      </lane>
    </frame>
  </panel>
</panel>

<template name="form-row">
  <lane layout="content content"
        vertical-content-alignment="middle">
    <label layout="140px content"
            margin="0,8"
            font="small"
            text={&title}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>

<template name="enum-segments">
  <frame background={@Mods/StardewUI/Sprites/MenuSlotTransparent} padding="4" tooltip="">
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
</template>

<template name="spin-box">
  <lane orientation="horizontal" vertical-content-alignment="middle" margin="4,0">
    <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
      left-click=|Decrease()|/>
    <label wheel=|Wheel($Direction)| text={Value}
      font="dialogue"
      layout="content[64..] content"
      padding="4,0,2,0"
      focusable="true"
      horizontal-alignment="middle"
      shadow-alpha="0.8"
      shadow-color="#4448"
      shadow-offset="-2, 2"/>
    <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
      left-click=|Increase()|/>
  </lane>
</template>
