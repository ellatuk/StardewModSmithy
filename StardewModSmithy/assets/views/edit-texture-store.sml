<frame layout="80%[1240..] 80%[680..]" 
    background={@Mods/StardewUI/Sprites/MenuBackground}
    border={@Mods/StardewUI/Sprites/MenuBorder}
    border-thickness="32, 36, 24, 36" padding="16">
  <grid item-layout="length: 224">
    <frame *repeat={Textures}
      border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
      border-thickness="4"
      background={@Mods/StardewUI/Sprites/White}
      background-tint="Transparent"
      focusable="true"
      left-click=|^SelectTextureAsset(this)|
      +transition:background-tint="100ms EaseOutCubic"
      +hover:background-tint="Wheat"
      +state:selected={IsSelected}
      +state:selected:background-tint="#39d"
      >
      <image
        layout="192px 192px"
        sprite={UISpriteSmall}
        fit="Cover"/>
    </frame>
  </grid>
</frame>
