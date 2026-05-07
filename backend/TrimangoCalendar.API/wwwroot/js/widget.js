(function() {
    'use strict';
    
    class HotelPlatformWidget {
        constructor() {
            this.widgetKey = null;
            this.apiUrl = 'https://yourdomain.com/widget/api';
            this.container = null;
            this.config = null;
            this.state = {
                step: 'search', // search, results, details, booking, confirmation
                searchParams: null,
                selectedUnit: null,
                bookingData: null
            };
        }
        
        init(widgetKey) {
            this.widgetKey = widgetKey;
            this.container = document.querySelector(`[data-widget-key="${widgetKey}"]`);
            
            if (!this.container) {
                console.error('Widget container bulunamadı');
                return;
            }
            
            this.loadConfig().then(() => {
                this.render();
                this.setupEventListeners();
            });
        }
        
        async loadConfig() {
            try {
                const response = await fetch(`${this.apiUrl}/config/${this.widgetKey}`);
                const data = await response.json();
                this.config = data.data;
                this.applyTheme();
            } catch (error) {
                console.error('Widget config yüklenemedi:', error);
            }
        }
        
        applyTheme() {
            if (!this.config) return;
            
            const style = document.createElement('style');
            style.textContent = `
                .hp-widget {
                    font-family: ${this.config.fontFamily};
                    --hp-primary: ${this.config.primaryColor};
                    --hp-secondary: ${this.config.secondaryColor};
                }
            `;
            document.head.appendChild(style);
        }
        
        render() {
            switch(this.state.step) {
                case 'search':
                    this.renderSearchForm();
                    break;
                case 'results':
                    this.renderSearchResults();
                    break;
                case 'details':
                    this.renderUnitDetails();
                    break;
                case 'booking':
                    this.renderBookingForm();
                    break;
                case 'confirmation':
                    this.renderConfirmation();
                    break;
            }
        }
        
        renderSearchForm() {
            const today = new Date().toISOString().split('T')[0];
            const tomorrow = new Date(Date.now() + 86400000).toISOString().split('T')[0];
            
            this.container.innerHTML = `
                <div class="hp-widget hp-widget-${this.config.position}">
                    <div class="hp-widget-header">
                        <h3>${this.config.metaTitle || 'Rezervasyon Yap'}</h3>
                    </div>
                    <div class="hp-widget-body">
                        <form id="hp-search-form">
                            <div class="hp-form-group">
                                <label>Giriş Tarihi</label>
                                <input type="date" id="hp-checkin" min="${today}" value="${today}" required>
                            </div>
                            <div class="hp-form-group">
                                <label>Çıkış Tarihi</label>
                                <input type="date" id="hp-checkout" min="${tomorrow}" value="${tomorrow}" required>
                            </div>
                            <div class="hp-form-row">
                                <div class="hp-form-group">
                                    <label>Yetişkin</label>
                                    <select id="hp-adults">
                                        <option value="1">1</option>
                                        <option value="2" selected>2</option>
                                        <option value="3">3</option>
                                        <option value="4">4</option>
                                    </select>
                                </div>
                                <div class="hp-form-group">
                                    <label>Çocuk</label>
                                    <select id="hp-children">
                                        <option value="0" selected>0</option>
                                        <option value="1">1</option>
                                        <option value="2">2</option>
                                        <option value="3">3</option>
                                    </select>
                                </div>
                            </div>
                            <button type="submit" class="hp-btn hp-btn-primary">
                                Müsaitliği Kontrol Et
                            </button>
                        </form>
                    </div>
                </div>
            `;
        }
        
        async handleSearch(event) {
            event.preventDefault();
            
            const searchParams = {
                checkIn: document.getElementById('hp-checkin').value,
                checkOut: document.getElementById('hp-checkout').value,
                adults: parseInt(document.getElementById('hp-adults').value),
                children: parseInt(document.getElementById('hp-children').value),
                currencyCode: 'TRY'
            };
            
            // Loading state
            const submitBtn = event.target.querySelector('button[type="submit"]');
            submitBtn.disabled = true;
            submitBtn.textContent = 'Aranıyor...';
            
            try {
                const response = await fetch(`${this.apiUrl}/search/${this.widgetKey}`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(searchParams)
                });
                
                const data = await response.json();
                
                if (data.success) {
                    this.state.searchParams = searchParams;
                    this.state.searchResults = data.data;
                    this.state.step = 'results';
                    this.render();
                } else {
                    alert(data.message || 'Bir hata oluştu');
                }
            } catch (error) {
                console.error('Arama hatası:', error);
                alert('Arama yapılırken bir hata oluştu');
            } finally {
                submitBtn.disabled = false;
                submitBtn.textContent = 'Müsaitliği Kontrol Et';
            }
        }
        
        renderSearchResults() {
            const results = this.state.searchResults;
            
            let unitsHtml = '';
            if (results.availableUnits.length === 0) {
                unitsHtml = `
                    <div class="hp-no-results">
                        <p>Üzgünüz, seçtiğiniz tarihlerde müsait birim bulunamadı.</p>
                        <button class="hp-btn hp-btn-secondary" onclick="window.hpw.goBack()">
                            Yeni Arama
                        </button>
                    </div>
                `;
            } else {
                results.availableUnits.forEach(unit => {
                    unitsHtml += `
                        <div class="hp-unit-card">
                            <div class="hp-unit-info">
                                <h4>${unit.unitName}</h4>
                                <div class="hp-unit-details">
                                    <span>👤 ${unit.maxAdults} Yetişkin</span>
                                    <span>🛏️ ${unit.maxChildren} Çocuk</span>
                                    ${unit.size ? `<span>📏 ${unit.size} m²</span>` : ''}
                                </div>
                                ${unit.amenities ? `
                                    <div class="hp-amenities">
                                        ${unit.amenities.slice(0, 5).map(a => `<span class="hp-amenity">✓ ${a}</span>`).join('')}
                                    </div>
                                ` : ''}
                            </div>
                            <div class="hp-unit-price">
                                <div class="hp-price">${unit.formattedTotalPrice}</div>
                                <div class="hp-price-per-night">${unit.formattedAveragePrice} / gece</div>
                                <button class="hp-btn hp-btn-primary" onclick="window.hpw.selectUnit('${unit.unitId}')">
                                    Seç
                                </button>
                            </div>
                        </div>
                    `;
                });
            }
            
            this.container.innerHTML = `
                <div class="hp-widget hp-widget-${this.config.position} hp-widget-results">
                    <div class="hp-widget-header">
                        <button class="hp-back-btn" onclick="window.hpw.goBack()">← Geri</button>
                        <h3>${results.propertyName}</h3>
                    </div>
                    <div class="hp-search-summary">
                        <span>📅 ${this.formatDate(results.checkIn)} - ${this.formatDate(results.checkOut)}</span>
                        <span>👤 ${results.adults} Yetişkin</span>
                        <span>🕐 ${results.totalNights} Gece</span>
                    </div>
                    <div class="hp-widget-body">
                        <div class="hp-units-list">
                            ${unitsHtml}
                        </div>
                    </div>
                </div>
            `;
        }
        
        selectUnit(unitId) {
            const unit = this.state.searchResults.availableUnits.find(u => u.unitId === unitId);
            if (!unit) return;
            
            this.state.selectedUnit = unit;
            this.state.step = 'details';
            this.render();
        }
        
        goBack() {
            if (this.state.step === 'results') {
                this.state.step = 'search';
            } else if (this.state.step === 'details') {
                this.state.step = 'results';
            } else if (this.state.step === 'booking') {
                this.state.step = 'details';
            }
            this.render();
        }
        
        formatDate(dateString) {
            return new Date(dateString).toLocaleDateString('tr-TR', {
                day: 'numeric',
                month: 'long',
                year: 'numeric'
            });
        }
        
        setupEventListeners() {
            this.container.addEventListener('submit', (e) => {
                if (e.target.id === 'hp-search-form') {
                    this.handleSearch(e);
                } else if (e.target.id === 'hp-booking-form') {
                    this.handleBooking(e);
                }
            });
        }
    }
    
    // Global instance
    window.hpw = new HotelPlatformWidget();
    
    // Auto-init
    document.addEventListener('DOMContentLoaded', () => {
        const widgets = document.querySelectorAll('[data-widget-key]');
        widgets.forEach(widget => {
            const key = widget.dataset.widgetKey;
            window.hpw.init(key);
        });
    });
})();
/* wwwroot/css/widget.css */
.hp-widget {
    font-family: var(--hp-font, 'Inter', sans-serif);
    max-width: 400px;
    background: white;
    border-radius: 12px;
    box-shadow: 0 4px 24px rgba(0,0,0,0.12);
    overflow: hidden;
}

.hp-widget-header {
    background: var(--hp-primary, #2563EB);
    color: white;
    padding: 16px 20px;
}

.hp-widget-header h3 {
    margin: 0;
    font-size: 18px;
    font-weight: 600;
}

.hp-widget-body {
    padding: 20px;
}

.hp-form-group {
    margin-bottom: 16px;
}

.hp-form-group label {
    display: block;
    font-size: 13px;
    font-weight: 500;
    color: #4B5563;
    margin-bottom: 6px;
}

.hp-form-group input,
.hp-form-group select {
    width: 100%;
    padding: 10px 12px;
    border: 1px solid #D1D5DB;
    border-radius: 8px;
    font-size: 14px;
    transition: border-color 0.2s;
}

.hp-form-group input:focus,
.hp-form-group select:focus {
    outline: none;
    border-color: var(--hp-primary, #2563EB);
    box-shadow: 0 0 0 3px rgba(37,99,235,0.1);
}

.hp-form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
}

.hp-btn {
    width: 100%;
    padding: 12px;
    border: none;
    border-radius: 8px;
    font-size: 15px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
}

.hp-btn-primary {
    background: var(--hp-primary, #2563EB);
    color: white;
}

.hp-btn-primary:hover {
    background: var(--hp-secondary, #1E40AF);
    transform: translateY(-1px);
}

.hp-btn-secondary {
    background: #F3F4F6;
    color: #374151;
}

.hp-unit-card {
    display: flex;
    justify-content: space-between;
    padding: 16px;
    border: 1px solid #E5E7EB;
    border-radius: 8px;
    margin-bottom: 12px;
    transition: border-color 0.2s;
}

.hp-unit-card:hover {
    border-color: var(--hp-primary, #2563EB);
}

.hp-unit-info h4 {
    margin: 0 0 8px 0;
    font-size: 16px;
    color: #1F2937;
}

.hp-unit-details {
    display: flex;
    gap: 12px;
    font-size: 13px;
    color: #6B7280;
    margin-bottom: 8px;
}

.hp-amenities {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.hp-amenity {
    font-size: 12px;
    color: #059669;
    background: #ECFDF5;
    padding: 2px 8px;
    border-radius: 4px;
}

.hp-unit-price {
    text-align: right;
    display: flex;
    flex-direction: column;
    justify-content: center;
    min-width: 120px;
}

.hp-price {
    font-size: 20px;
    font-weight: 700;
    color: var(--hp-primary, #2563EB);
}

.hp-price-per-night {
    font-size: 12px;
    color: #6B7280;
    margin: 4px 0 8px 0;
}

.hp-search-summary {
    display: flex;
    gap: 16px;
    padding: 12px 20px;
    background: #F9FAFB;
    font-size: 13px;
    color: #4B5563;
    border-bottom: 1px solid #E5E7EB;
}

.hp-back-btn {
    background: none;
    border: none;
    color: white;
    font-size: 14px;
    cursor: pointer;
    padding: 0;
    margin-bottom: 8px;
}

.hp-no-results {
    text-align: center;
    padding: 40px 20px;
}

.hp-no-results p {
    color: #6B7280;
    margin-bottom: 16px;
}

/* Responsive */
@media (max-width: 480px) {
    .hp-widget {
        max-width: 100%;
        border-radius: 0;
    }
}
html
@* Views/Widget/WidgetIndex.cshtml *@
@{
    Layout = null;
    var widgetKey = Model as string;
}

<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Rezervasyon</title>
    <link rel="stylesheet" href="/widget/css/widget.css">
    <style>
        body {
            margin: 0;
            padding: 20px;
            background: #F3F4F6;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }
    </style>
</head>
<body>
    <div id="hp-booking-widget" data-widget-key="@widgetKey"></div>
    <script src="/widget/js/widget.js"></script>
</body>
</html>
