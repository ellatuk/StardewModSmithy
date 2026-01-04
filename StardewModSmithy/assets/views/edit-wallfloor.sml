<panel layout="100% 100%" horizontal-content-alignment="end">
  <include name="mushymato.StardewModSmithy/views/includes/draggable-texture-sheet" *context={:TextureContext}/>
  <lane layout="500px content" orientation="vertical" *context={:EditableContext}>
    <frame layout="stretch content" padding="30,20,0,20" border={@Mods/StardewUI/Sprites/ControlBorder}>
      <lane *context={:BoundsProviderSelector} orientation="horizontal"
        vertical-content-alignment="middle"
        margin="4,12"
        clip-size="content 64px">
        <image sprite={@Mods/StardewUI/Sprites/CaretLeft} focusable="true"
          left-click=|Decrease()|
          +hover:scale="1.1"/>
        <label wheel=|Wheel($Direction)| text={ValueLabel}
          font="dialogue"
          layout="280px content"
          padding="24,0,22,0"
          focusable="true"
          horizontal-alignment="middle"
          shadow-alpha="0.8"
          shadow-color="#4448"
          shadow-offset="-2, 2"
          max-lines="1"/>
        <image sprite={@Mods/StardewUI/Sprites/CaretRight} focusable="true"
          left-click=|Increase()|
          +hover:scale="1.1"/>
      </lane>
    </frame>
  </lane>
</panel>
