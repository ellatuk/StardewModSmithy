<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <lane layout="500px content" orientation="vertical" *context={:EditableContext}>
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
       <lane *context={:BoundsProviderSelector}
          orientation="vertical"
          layout="stretch content[..1000]"
          vertical-content-alignment="middle"
          margin="4,4">
          <lane orientation="horizontal" vertical-content-alignment="middle" padding="0,16">
            <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
              left-click=|Decrease()|
              +hover:scale="1.2"
              +transition:scale="100ms EaseOutCubic"/>
            <label text={ValueLabel}
              font="dialogue"
              layout="stretch content"
              padding="20,0"
              focusable="true"
              horizontal-alignment="middle"
              shadow-alpha="0.8"
              shadow-color="#4448"
              shadow-offset="-2, 2"
              max-lines="1"
              wheel=|Wheel($Direction)|
              left-click=|ToggleViewingBoundsProviderList()|/>
            <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
              left-click=|Increase()|
              +hover:scale="1.2"
              +transition:scale="100ms EaseOutCubic"/>
          </lane>
          <textinput *if={ViewingBoundsProviderList}
            text={<>BoundsProviderSearchTerm}
            font="dialogue" layout="content 64px" margin="-4,0,0,0"/>
          <scrollable *if={ViewingBoundsProviderList} peeking="128" scrollbar-margin="8,0,0,0">
            <lane orientation="vertical" layout="content content">
              <frame *repeat={FilteredBoundsProviderList}
                border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
                border-thickness="4"
                layout="stretch content"
                margin="2"
                padding="8"
                background={@Mods/StardewUI/Sprites/White}
                background-tint="Transparent"
                +state:selected={:IsSelected}
                +state:selected:background-tint="#39d"
                +hover:background-tint="Wheat"
                left-click=|^SelectBoundsProvider(this)|>
                <label text={:UILabel}
                  font="dialogue"
                  focusable="true"
                  horizontal-alignment="start"
                  shadow-alpha="0.8"
                  shadow-color="#4448"
                  shadow-offset="-2, 2"
                  max-lines="1"/>
              </frame>
            </lane>
          </scrollable>
        </lane>
        <lane layout="stretch content" horizontal-content-alignment="start" >
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="50% 56px"
            margin="0,0,10,0"
            text={#gui.button.create}
            left-click=|Create()|
          />
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="100% 56px"
            text={#gui.button.delete}
            left-click=|Delete()|
          />
        </lane>
        <panel layout="stretch content" horizontal-content-alignment="start" >
          <button hover-background={@Mods/StardewUI/Sprites/ButtonLight}
            layout="stretch[320..] 56px"
            margin="0,4,0,0"
            text={#gui.button.save}
            left-click=|Save()|
          />
        </panel>
        <label layout="stretch content" opacity="0.6" text={LastSavedMessage}/>
      </lane>
    </frame>
    <frame layout="stretch content" padding="32,16,32,32" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasBoundsProvider}>
      <lane orientation="vertical" *context={SelectedFurniture}>
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
          <lane orientation="vertical">
            <textinput layout="stretch 54px" margin="-8,12" text={<>PriceInput} />
            <checkbox label-text={#gui.label.no-random-sale} is-checked={<>OffLimitsForRandomSale}/>
            <checkbox label-text={#gui.label.is-catalogue} is-checked={<>IsCatalogue}/>
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
