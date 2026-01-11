<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <lane layout="500px content" orientation="vertical" *context={:EditableContext}>
    <!-- General Controls -->
    <include name="mushymato.StardewModSmithy/views/includes/general-controls" />
    <!-- wallpaper/floor Edit -->
    <frame layout="stretch content" padding="32,24" border={@Mods/StardewUI/Sprites/ControlBorder} *if={HasBoundsProvider}>
      <lane orientation="vertical" *context={Selected}>
        <form-row title={#gui.label.count}>
          <lane *context={Count} orientation="horizontal" vertical-content-alignment="middle" margin="4,0">
            <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
              left-click=|Decrease()|
              +hover:scale="1.2"
              +transition:scale="100ms EaseOutCubic"/>
            <label wheel=|Wheel($Direction)| text={ValueLabel}
              font="dialogue"
              layout="content[80..] content"
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
        </form-row>
        <form-row title={#gui.label.kind}>
          <frame background={@Mods/StardewUI/Sprites/MenuSlotOutset} padding="4">
            <segments balanced="true"
                highlight={@Mods/StardewUI/Sprites/White}
                highlight-tint="#39d"
                highlight-transition="150ms EaseOutQuart"
                separator={@Mods/StardewUI/Sprites/White}
                separator-tint="#c99"
                separator-width="2"
                selected-index={<>WallOrFloor}>
              <label margin="12, 8" text={#gui.label.wallpaper}/>
              <label margin="12, 8" text={#gui.label.flooring}/>
            </segments>
          </frame>
        </form-row>
      </lane>
    </frame>
  </lane>
</panel>

<template name="form-row">
  <lane layout="content content"  vertical-content-alignment="middle" margin="0,8">
    <label layout="120px content"
            margin="0,8"
            font="small"
            text={&title}
            shadow-alpha="0.8"
            shadow-color="#4448"
            shadow-offset="-2, 2" />
    <outlet />
  </lane>
</template>
