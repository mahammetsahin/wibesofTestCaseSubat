# Building Selection UI Setup Guide

This guide will help you set up the UI for selecting and placing buildings in your grid-based game.

## Creating the Building Selection Panel

1. **Create a Canvas**:
   - Right-click in the Hierarchy and select UI > Canvas
   - Make sure the Canvas has a Canvas Scaler component set to "Scale With Screen Size"

2. **Create a Building Selection Panel**:
   - Right-click on the Canvas and select UI > Panel
   - Rename it to "BuildingSelectionPanel"
   - Position it at the bottom or side of the screen
   - Adjust its size and appearance as needed

3. **Add Building Buttons**:
   - Right-click on the BuildingSelectionPanel and select UI > Button
   - Create multiple buttons for different building types
   - Rename each button according to the building type (e.g., "HouseButton", "FactoryButton")
   - Add icons or text to each button to represent the building type

4. **Add the BuildingSelectionUI Script**:
   - Select the BuildingSelectionPanel
   - Add the BuildingSelectionUI component
   - Assign your GameManager to the "Game Manager" field

5. **Configure Building Buttons**:
   - In the BuildingSelectionUI component, set the size of the Building Buttons list to match your number of buttons
   - For each entry in the list:
     - Set the Building Name (e.g., "House", "Factory")
     - Assign the Building Prefab from your project's Resources or Assets folder
     - Drag the corresponding UI Button from the hierarchy to the UI Button field

## Building Prefab Requirements

Each building prefab should have:
1. A PlaceableObject or Building component
2. Properly configured Size property (Vector2Int)
3. Materials for both valid and invalid placement states

## Example Setup

Here's an example of how your hierarchy might look:

```
Canvas
└── BuildingSelectionPanel (BuildingSelectionUI component)
    ├── HouseButton
    ├── FactoryButton
    ├── WarehouseButton
    └── ...
```

And the BuildingSelectionUI component configuration:

```
Building Buttons:
- Element 0
  - Building Name: House
  - Building Prefab: [House Prefab]
  - UI Button: [HouseButton]
- Element 1
  - Building Name: Factory
  - Building Prefab: [Factory Prefab]
  - UI Button: [FactoryButton]
- ...
```

## Testing

1. Enter Play mode
2. Click on a building button
3. The building should appear and follow your mouse cursor
4. Click to place the building on a valid grid position

## Troubleshooting

- If buttons don't respond, check that the Button component has the correct On Click event
- If buildings don't appear, verify that the prefabs have the correct components
- If the GameManager reference is missing, make sure it's assigned in the inspector 

## Turkish Localization (Türkçe Yerelleştirme)

# Bina Seçim Arayüzü Kurulum Rehberi

Bu rehber, ızgara tabanlı oyununuzda binaları seçmek ve yerleştirmek için arayüz kurulumunu yapmanıza yardımcı olacaktır.

## Bina Seçim Paneli Oluşturma

1. **Canvas Oluşturma**:
   - Hiyerarşide sağ tıklayın ve UI > Canvas seçin
   - Canvas'ın "Scale With Screen Size" olarak ayarlanmış bir Canvas Scaler bileşenine sahip olduğundan emin olun

2. **Bina Seçim Paneli Oluşturma**:
   - Canvas üzerinde sağ tıklayın ve UI > Panel seçin
   - "BuildingSelectionPanel" olarak yeniden adlandırın
   - Ekranın altına veya kenarına konumlandırın
   - Boyutunu ve görünümünü gerektiği gibi ayarlayın

3. **Bina Düğmeleri Ekleme**:
   - BuildingSelectionPanel üzerinde sağ tıklayın ve UI > Button seçin
   - Farklı bina türleri için birden fazla düğme oluşturun
   - Her düğmeyi bina türüne göre yeniden adlandırın (örn. "HouseButton", "FactoryButton")
   - Bina türünü temsil etmek için her düğmeye simgeler veya metin ekleyin

4. **BuildingSelectionUI Komut Dosyasını Ekleme**:
   - BuildingSelectionPanel'i seçin
   - BuildingSelectionUI bileşenini ekleyin
   - "Game Manager" alanına GameManager'ınızı atayın

5. **Bina Düğmelerini Yapılandırma**:
   - BuildingSelectionUI bileşeninde, Building Buttons listesinin boyutunu düğme sayınıza uygun olarak ayarlayın
   - Listedeki her giriş için:
     - Building Name'i ayarlayın (örn. "House", "Factory")
     - Building Prefab'ı projenizin Resources veya Assets klasöründen atayın
     - UI Button alanına hiyerarşiden ilgili UI Button'ı sürükleyin

## Bina Prefab Gereksinimleri

Her bina prefabı şunlara sahip olmalıdır:
1. PlaceableObject veya Building bileşeni
2. Doğru yapılandırılmış Size özelliği (Vector2Int)
3. Hem geçerli hem de geçersiz yerleştirme durumları için materyaller

## Örnek Kurulum

Hiyerarşinizin nasıl görünebileceğine dair bir örnek:

```
Canvas
└── BuildingSelectionPanel (BuildingSelectionUI bileşeni)
    ├── HouseButton
    ├── FactoryButton
    ├── WarehouseButton
    └── ...
```

Ve BuildingSelectionUI bileşeni yapılandırması:

```
Building Buttons:
- Element 0
  - Building Name: House
  - Building Prefab: [House Prefab]
  - UI Button: [HouseButton]
- Element 1
  - Building Name: Factory
  - Building Prefab: [Factory Prefab]
  - UI Button: [FactoryButton]
- ...
```

## Test Etme

1. Play moduna girin
2. Bir bina düğmesine tıklayın
3. Bina görünmeli ve fare imlecini takip etmeli
4. Geçerli bir ızgara konumuna yerleştirmek için tıklayın

## Sorun Giderme

- Düğmeler yanıt vermiyorsa, Button bileşeninin doğru On Click olayına sahip olduğunu kontrol edin
- Binalar görünmüyorsa, prefabların doğru bileşenlere sahip olduğunu doğrulayın
- GameManager referansı eksikse, denetçide atandığından emin olun 