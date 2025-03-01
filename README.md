# Grid System Implementation

This document explains the grid system implementation for the farming game.

## Overview

The grid system is implemented as a 2D grid on a plane where players can place buildings and crop fields. The system supports:

- Editor-configurable grid size and cell size
- Two types of placeable objects: Buildings and Crop Fields
- Buildings that can span multiple grid cells (configurable size)
- Crop Fields that are always 1x1 cell
- Drag and drop placement from UI
- Moving existing buildings by holding for 1 second
- All objects levitate while being dragged for better visibility
- Buildings permanently hover above the ground
- Validation to prevent placing objects on occupied cells
- Crop planting, growing, and harvesting mechanics with simple click interaction
- Mobile touch input support

## Core Components

### GameManager

The central script that manages both the grid system and object placement. It handles:
- Grid creation and management
- Object placement validation
- User input for placing, moving, and interacting with objects (mouse and touch)
- Crop field interactions and UI management
- Object hover effects during dragging and placement

### PlaceableObject

Base class for all objects that can be placed on the grid. Features:
- Configurable size (Vector2Int)
- Visual feedback for valid/invalid placement
- Grid position tracking
- Placement event hooks

### Building

Extends PlaceableObject to represent structures. Features:
- Can span multiple grid cells
- Can be moved after placement by holding for 1 second
- Permanently hovers above the ground for better visibility
- Visual feedback during movement

### CropField

Extends PlaceableObject to represent farmable land. Features:
- Always 1x1 cell size
- Stores empty, planted, and grown crop textures
- Tracks crop growth time
- Handles crop planting, growing, and harvesting
- Simple click interaction for planting and harvesting

### CropSelectionUI

UI component for selecting which crop to plant. Features:
- Buttons for different crop types
- Direct planting when a crop type is selected
- Communication with GameManager to plant selected crop

### BuildingSelectionUI

UI component for selecting buildings to place. Features:
- Configurable list of building buttons
- Each button linked to a specific building prefab
- Handles instantiation and placement of buildings through GameManager

## Usage Instructions

1. **Grid Setup**:
   - Adjust grid width, height, and cell size in the GameManager inspector
   - Assign a plane transform to represent the grid visually

2. **Creating Placeable Objects**:
   - Create prefabs that use either Building or CropField components
   - Configure size for buildings (CropFields are always 1x1)
   - Assign visual materials for default and invalid placement states

3. **UI Setup**:
   - Add the BuildingSelectionUI component to your building selection panel
   - Configure building buttons by assigning prefabs and UI buttons
   - Set up CropSelectionUI with buttons for different crop types

4. **Crop Configuration**:
   - Assign textures for empty fields, planted crops, and grown crops
   - Configure growth times for different crop types

5. **Hover Effects**:
   - Adjust the `dragHoverHeight` parameter to control how high objects float while being dragged
   - Adjust the `buildingHoverHeight` parameter to control how high buildings permanently float
   - Default value for both is 0.5 units

6. **Mobile Configuration**:
   - The system automatically supports both mouse and touch input
   - No additional configuration is needed for mobile deployment

## Interaction Flow

1. **Placing Objects**:
   - Press and hold on a building or crop button in the UI
   - Drag away from the button to instantiate the object (must drag at least 20 pixels)
   - The object appears under your pointer/finger and is centered on the pointer while dragging
   - Drag to desired position (object will levitate while being dragged)
   - Valid positions show normal color, invalid positions show red
   - Release to place if position is valid (object will snap to grid with its pivot at the corner)
   - If the position is invalid or off the grid, the object will be destroyed
   - Buildings will remain elevated, other objects will return to ground level

2. **Moving Buildings**:
   - Click/tap and hold on a building for 1 second
   - Drag to new position (building will hover while being moved and is centered on the pointer)
   - Release to place if position is valid (building will snap to grid with its pivot at the corner), otherwise returns to original position
   - Building maintains its hover height after placement

3. **Crop Field Interaction**:
   - Click/tap on empty crop field to open crop selection UI
   - Click/tap on a crop type button to immediately plant that crop
   - Click/tap on planted crop field to see growth timer
   - Click/tap on grown crop to harvest

## Implementation Notes

- The grid uses a Dictionary<Vector2Int, GridCell> for efficient cell lookup
- DateTime is used to track crop growth times
- Coroutines update UI elements like the crop timer
- Raycasting is used for grid cell selection
- All objects levitate during dragging for better visibility
- Only buildings permanently hover above the ground after placement
- Two separate height parameters control dragging elevation and building permanent elevation
- Input handling supports both mouse and touch input for cross-platform compatibility
- Objects are instantiated using a drag-from-button gesture rather than a simple click/tap
- New objects are automatically destroyed if they can't be placed in a valid position or if dragged off the grid
- Existing objects return to their original position if they can't be placed in a new position
- Crop fields use a simple click interaction model for planting and harvesting 

## Grid System

The game uses a grid-based system for placing objects. Key features:

- Grid dimensions: 10x10 cells by default
- Objects are placed using corner-based positioning (not center-based)
- Buildings and other objects snap to grid corners when placed
- Buildings hover slightly above the ground
- Objects can be moved by holding and dragging
- Visual feedback shows valid/invalid placement

### Grid Sistemi Uygulaması

Bu belge, çiftlik oyunu için ızgara sistemi uygulamasını açıklar.

### Genel Bakış

Grid sistemi, oyuncuların binalar ve ekin alanları yerleştirebileceği bir düzlem üzerinde 2B ızgara olarak uygulanır. Sistem şunları destekler:

- Editörde yapılandırılabilir ızgara boyutu ve hücre boyutu
- İki tür yerleştirilebilir nesne: Binalar ve Ekin Alanları
- Birden fazla ızgara hücresine yayılabilen binalar (yapılandırılabilir boyut)
- Her zaman 1x1 hücre olan Ekin Alanları
- Arayüzden sürükle ve bırak yerleştirme
- 1 saniye basılı tutarak mevcut binaları taşıma
- Daha iyi görünürlük için sürüklenirken havada süzülen tüm nesneler
- Kalıcı olarak yerden yüksekte duran binalar
- Dolu hücrelere nesnelerin yerleştirilmesini önleyen doğrulama
- Basit tıklama etkileşimi ile ekin dikme, büyütme ve hasat mekanikleri
- Mobil dokunmatik giriş desteği

### Izgara Sistemi

Oyun, nesneleri yerleştirmek için ızgara tabanlı bir sistem kullanır. Temel özellikler:

- Izgara boyutları: Varsayılan olarak 10x10 hücre
- Nesneler merkez tabanlı değil, köşe tabanlı konumlandırma kullanılarak yerleştirilir
- Binalar ve diğer nesneler yerleştirildiğinde ızgara köşelerine yapışır
- Binalar yerden biraz yüksekte durur
- Nesneler basılı tutularak ve sürüklenerek taşınabilir
- Görsel geri bildirim, geçerli/geçersiz yerleşimi gösterir

### Temel Bileşenler

#### GameManager

Hem ızgara sistemini hem de nesne yerleştirmeyi yöneten merkezi komut dosyası. Şunları yönetir:
- Grid oluşturma ve yönetim
- Nesne yerleştirme doğrulaması
- Nesneleri yerleştirmek, taşımak ve etkileşimde bulunmak için kullanıcı girişi (fare ve dokunmatik)
- Ekin alanı etkileşimleri ve arayüz yönetimi
- Sürükleme ve yerleştirme sırasında nesne havada süzülme efektleri

#### PlaceableObject

Gride yerleştirilebilen tüm nesneler için temel sınıf. Özellikler:
- Yapılandırılabilir boyut (Vector2Int)
- Geçerli/geçersiz yerleştirme için görsel geri bildirim
- Grid konumu takibi
- Yerleştirme olay kancaları

#### Building

Yapıları temsil etmek için PlaceableObject'i genişletir. Özellikler:
- Birden fazla ızgara hücresine yayılabilir
- 1 saniye basılı tutarak yerleştirmeden sonra taşınabilir
- Daha iyi görünürlük için kalıcı olarak yerden yüksekte durur
- Hareket sırasında görsel geri bildirim

#### CropField

Ekilebilir araziyi temsil etmek için PlaceableObject'i genişletir. Özellikler:
- Her zaman 1x1 hücre boyutu
- Boş, ekilmiş ve büyümüş ekin dokularını saklar
- Ekin büyüme süresini takip eder
- Ekin dikme, büyütme ve hasat işlemlerini yönetir
- Dikmek ve hasat etmek için basit tıklama etkileşimi

#### CropSelectionUI

Hangi ekinin dikileceğini seçmek için arayüz bileşeni. Özellikler:
- Farklı ekin türleri için düğmeler
- Bir ekin türü seçildiğinde doğrudan dikim
- Seçilen ekini dikmek için GameManager ile iletişim

#### BuildingSelectionUI

Yerleştirilecek binaları seçmek için arayüz bileşeni. Özellikler:
- Yapılandırılabilir bina düğmeleri listesi
- Her düğme belirli bir bina prefabına bağlı
- GameManager aracılığıyla binaların örneklenmesi ve yerleştirilmesi

### Kullanım Talimatları

1. **Grid Kurulumu**:
   - GameManager denetçisinde ızgara genişliğini, yüksekliğini ve hücre boyutunu ayarlayın
   - Gridi görsel olarak temsil etmek için bir düzlem dönüşümü atayın

2. **Yerleştirilebilir Nesneler Oluşturma**:
   - Building veya CropField bileşenlerini kullanan prefablar oluşturun
   - Binalar için boyutu yapılandırın (CropFields her zaman 1x1'dir)
   - Varsayılan ve geçersiz yerleştirme durumları için görsel materyaller atayın

3. **Arayüz Kurulumu**:
   - Bina seçim panelinize BuildingSelectionUI bileşenini ekleyin
   - Prefabları ve arayüz düğmelerini atayarak bina düğmelerini yapılandırın
   - Farklı ekin türleri için düğmelerle CropSelectionUI'yi kurun

4. **Ekin Yapılandırması**:
   - Boş alanlar, ekilmiş ekinler ve büyümüş ekinler için dokular atayın
   - Farklı ekin türleri için büyüme sürelerini yapılandırın

5. **Havada Süzülme Efektleri**:
   - Nesnelerin sürüklenirken ne kadar yüksekte süzüleceğini kontrol etmek için `dragHoverHeight` parametresini ayarlayın
   - Binaların kalıcı olarak ne kadar yüksekte süzüleceğini kontrol etmek için `buildingHoverHeight` parametresini ayarlayın
   - Her ikisi için de varsayılan değer 0.5 birimdir

6. **Mobil Yapılandırma**:
   - Sistem otomatik olarak hem fare hem de dokunmatik girişi destekler
   - Mobil dağıtım için ek yapılandırma gerekmez

### Etkileşim Akışı

1. **Nesneleri Yerleştirme**:
   - Arayüzdeki bir bina veya ekin düğmesine basın ve basılı tutun
   - Nesneyi örneklemek için düğmeden uzağa sürükleyin (en az 20 piksel sürüklemeniz gerekir)
   - Nesne işaretçinizin/parmağınızın altında görünür ve sürükleme sırasında işaretçiye ortalanır
   - İstediğiniz konuma sürükleyin (nesne sürüklenirken havada süzülecektir)
   - Geçerli konumlar normal renkte gösterilir, geçersiz konumlar kırmızı gösterilir
   - Konum geçerliyse yerleştirmek için bırakın (nesne yerleştirildiğinde ızgaraya köşe noktasından yapışacaktır)
   - Konum geçersizse veya ızgara dışındaysa, nesne yok edilecektir
   - Binalar yüksekte kalacak, diğer nesneler yer seviyesine dönecektir

2. **Binaları Taşıma**:
   - Bir binaya 1 saniye boyunca tıklayın/dokunun ve basılı tutun
   - Yeni konuma sürükleyin (bina taşınırken havada süzülecek ve işaretçiye ortalanacaktır)
   - Konum geçerliyse yerleştirmek için bırakın (bina ızgaraya köşe noktasından yapışacaktır), aksi takdirde orijinal konumuna döner
   - Bina yerleştirmeden sonra havada süzülme yüksekliğini korur

3. **Ekin Alanı Etkileşimi**:
   - Ekin seçim arayüzünü açmak için boş ekin alanına tıklayın/dokunun
   - O ekini hemen dikmek için bir ekin türü düğmesine tıklayın/dokunun
   - Büyüme zamanlayıcısını görmek için ekilmiş ekin alanına tıklayın/dokunun
   - Hasat etmek için büyümüş ekine tıklayın/dokunun

### Uygulama Notları

- Izgara, verimli hücre arama için Dictionary<Vector2Int, GridCell> kullanır
- Ekin büyüme sürelerini takip etmek için DateTime kullanılır
- Coroutine'ler ekin zamanlayıcısı gibi arayüz öğelerini günceller
- Izgara hücresi seçimi için ışın izleme kullanılır
- Daha iyi görünürlük için tüm nesneler sürükleme sırasında havada süzülür
- Yerleştirmeden sonra sadece binalar kalıcı olarak yerden yüksekte durur
- İki ayrı yükseklik parametresi, sürükleme yüksekliğini ve binaların kalıcı yüksekliğini kontrol eder
- Giriş işleme, platformlar arası uyumluluk için hem fare hem de dokunmatik girişi destekler
- Nesneler basit bir tıklama/dokunma yerine düğmeden sürükleme hareketiyle örneklenir
- Geçerli bir konuma yerleştirilemezse veya ızgara dışına sürüklenirse yeni nesneler otomatik olarak yok edilir
- Mevcut nesneler, yeni bir konuma yerleştirilemezse orijinal konumlarına geri döner
- Ekin alanları, dikim ve hasat için basit bir tıklama etkileşim modeli kullanır 