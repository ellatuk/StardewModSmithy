<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <lane layout="500px content" orientation="vertical" *context={:EditableContext}>
    <!-- General Controls -->
    <include name="mushymato.StardewModSmithy/views/includes/general-controls" />
    <!-- wallpaper/floor Edit -->
    <frame layout="stretch content" padding="32,24,32,32" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasBoundsProvider}>
      <lane orientation="vertical" *context={Selected}>
        <form-row title={#gui.label.count}>
          <spin-box *context={Count} />
        </form-row>
        <checkbox margin="8" label-text={#gui.label.is-flooring} is-checked={<>IsFlooring}/>
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
