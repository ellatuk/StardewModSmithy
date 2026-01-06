<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <lane layout="500px content" orientation="vertical" *context={:EditableContext}>
    <!-- General Controls -->
    <frame layout="stretch content" padding="30,20" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <lane orientation="vertical">
        <panel *if={TextureHasAtlas} layout="stretch content" horizontal-content-alignment="start">
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="stretch 56px"
            margin="0,4,0,0"
            text={#gui.button.populate}
            left-click=|PopulateFromAtlas()|
          />
        </panel>
        <include name="mushymato.StardewModSmithy/views/includes/bounds-provider-selector" *context={:BoundsProviderSelector}/>
        <grid layout="stretch content" item-layout="count: 2" item-spacing="4,4" >
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="stretch 56px"
            text={#gui.button.create}
            left-click=|Create()|
          />
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="stretch 56px"
            text={#gui.button.delete}
            left-click=|Delete()|
          />
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="stretch 56px"
            text={#gui.button.save}
            left-click=|Save()|
          />
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="stretch 56px"
            text={#gui.button.exit}
            left-click=|Exit()|
          />
        </grid>
        <label layout="stretch content" opacity="0.6" text={LastSavedMessage}/>
      </lane>
    </frame>
    <!-- Furniture Edit -->
    <frame layout="stretch content" padding="32,16,32,32" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasBoundsProvider}>
      <lane orientation="vertical" *context={Selected}>
        <form-row title={#gui.label.name}>
          <textinput text={<>DisplayName} font="dialogue" layout="content 64px" margin="-8,12" />
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
          <textinput layout="stretch 54px" margin="-8,12" text={<>PriceInput} />
        </form-row>
        <checkbox margin="8" label-text={#gui.label.no-random-sale} is-checked={<>OffLimitsForRandomSale}/>
        <checkbox margin="8" label-text={#gui.label.is-catalogue} is-checked={<>IsCatalogue}/>
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
      left-click=|Decrease()|
      +hover:scale="1.2"
      +transition:scale="100ms EaseOutCubic"/>
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
      left-click=|Increase()|
      +hover:scale="1.2"
      +transition:scale="100ms EaseOutCubic"/>
  </lane>
</template>
