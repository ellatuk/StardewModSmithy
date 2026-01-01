<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <lane layout="500px content" orientation="vertical" *context={:EditableContext}>
    <frame layout="stretch content" padding="30,20,0,20" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <lane orientation="vertical">
        <lane layout="content content"
            vertical-content-alignment="middle">
          <label layout="60px content"
            margin="0,8"
            font="small"
            text={#gui.label.id}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
          <lane *context={:BoundsProviderSelector} orientation="horizontal"
            vertical-content-alignment="middle"
            margin="4,12"
            clip-size="content 64px">
            <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
              left-click=|Decrease()|/>
            <label wheel=|Wheel($Direction)| text={ValueLabel}
              font="dialogue"
              layout="320px content"
              padding="4,0,2,0"
              focusable="true"
              horizontal-alignment="middle"
              shadow-alpha="0.8"
              shadow-color="#4448"
              shadow-offset="-2, 2"/>
            <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
              left-click=|Increase()|/>
          </lane>
        </lane>
        <lane layout="stretch content" horizontal-content-alignment="start" margin="92,0,0,0">
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="content[155..] 56px"
            margin="0,0,10,0"
            text={#gui.button.create}
            left-click=|Create()|
          />
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="content[155..] 56px"
            text={#gui.button.delete}
            left-click=|Delete()|
          />
        </lane>
        <lane *if={TextureHasAtlas} layout="stretch content" horizontal-content-alignment="start" margin="92,0,0,0">
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="content[320..] 56px"
            margin="0,4,0,0"
            text={#gui.button.populate}
            left-click=|PopulateFromAtlas()|
          />
        </lane>
      </lane>
    </frame>
    <frame layout="stretch stretch" padding="32,8" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasBoundsProvider}>
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
        <form-row title={#gui.label.price}>
          <lane orientation="vertical">
            <textinput layout="stretch 54px" margin="-8,12" text={<>PriceInput} />
            <checkbox label-text={#gui.label.no-random-sale} is-checked={<>OffLimitsForRandomSale}/>
          </lane>
        </form-row>
      </lane>
    </frame>
  </lane>
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

<template name="spin-box">
  <lane orientation="horizontal" vertical-content-alignment="middle" margin="4,0">
    <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
      left-click=|Decrease()|/>
    <label wheel=|Wheel($Direction)| text={ValueLabel}
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
