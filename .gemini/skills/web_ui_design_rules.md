# UI Design Skill: Web UI Design Rules

This document defines the strict UI/UX guidelines and rules that all AI coding assistants (including Antigravity) **MUST** follow when generating or modifying Web UI code (HTML, CSS, CSHTML, Javascript) in the **ZLearn** repository.

---

## 🚀 How to Apply This Skill
Before modifying any `.cshtml`, `.html`, or `.css` file in `ZLearn.Web`, read this rule set to ensure visual harmony, proper component structure, and layout integrity.

---

## 🎨 Rule 1: Use Design Tokens & Variables (Strictly No Hardcoded Colors)
You **MUST** use CSS variables from [site.css](file:///d:/projects/zlearn/ZLearn.Web/wwwroot/css/site.css) for all styling. Never hardcode colors like `#095a71` or `#00B0F0` in CSS or inline styles.

*   `var(--main-color)` (Primary Deep Teal) for headers, footers, primary actions, and active/selected tabs.
*   `var(--secondary-color)` (Accent Bright Blue) for highlight borders, focus rings, selected choices, and tags.
*   `var(--bg-color)` (Light Gray) for page and body background.
*   `var(--success-color)` (Green) for positive outcomes, correct answers, success tags, and alerts.
*   `var(--error-color)` (Red) for countdown alerts, wrong answers, deletes, and errors.

---

## 🧱 Rule 2: Follow Grid & Spacing Conventions
*   **Grid Framework:** Use Bootstrap 5 grid classes exclusively (`container`, `row`, `col-lg-8`, `col-lg-4`, `col-md-*`, `col-sm-*`).
*   **Spacing:** Use Bootstrap margin/padding utility classes (`my-4`, `mb-3`, `gap-3`, `me-2`, etc.) to maintain consistent spacing. Never write arbitrary inline pixel margins/paddings unless absolutely necessary for custom animations.

---

## 🏢 Rule 3: Adhere to Main Layout Structure
When creating or editing page views, you **MUST** respect the global page sections from [_Layout.cshtml](file:///d:/projects/zlearn/ZLearn.Web/Views/Shared/_Layout.cshtml):
*   **Main Container:** Always wrap main page content in a `<div class="container my-4">` (or similar container utility classes) to align with the grid system.
*   **Min-Height Constraint:** Ensure main content wrappers maintain a minimum height using `min-height: 100vh;` or container equivalents to prevent the teal footer from being pulled upwards on short pages.
*   **Active Tabs Styling:** If you build navigation components, always style inactive tabs with `.nav-tab` (60% opacity, changes to teal on hover) and active tabs with `.nav-tab.selected` (100% opacity, bottom border of 3px solid `var(--main-color)`).
*   **User Details Display:** Any custom user avatar must be `36x36px` (`rounded-pill object-fit-cover`). Names should be styled with `fw-light text-nowrap` and limited to `120px` max-width with ellipsis (`text-overflow: ellipsis`) if placed in tight navbar sections.

---

## ⚡ Rule 4: Enforce Interactive States & Hover Effects
All interactive elements (buttons, link cards, list items) must have explicit hover states:
*   Use `.opacity-hover` on non-bordered icons/links to fade them slightly (`opacity: 0.6; cursor: pointer`).
*   Use `.highlight-hover` on block elements, action buttons, and cards to trigger the secondary blue outline glow (`box-shadow: 0px 0px 5px var(--secondary-color) !important`) and full content opacity on hover.
*   Use `.hover-shadow` on list items or block cards to add a subtle default shadow effect.

---

## 📐 Rule 5: Match UI Component Specifications

### 5.1. Buttons
*   **Flat White Action Buttons:** Use the class combo `.btn .bg-white .border .rounded-0 .shadow-sm .highlight-hover`. Note that they must have `rounded-0` (perfectly square corners) and highlight-hover.
*   **Primary Action Buttons:** Use `.btn .btn-primary` and include appropriate FontAwesome icons (e.g. `<i class="fa-solid fa-play me-2"></i>`).

### 5.2. Status & Meta Tags
When displaying category names, difficulty levels, status tags, or counts, use our custom transparent tags:
*   **Info:** `.info-tag` (blue accent border + semi-transparent blue bg).
*   **Success:** `.success-tag` (green border + semi-transparent green bg).
*   **Error/Danger:** `.error-tag` (red border + semi-transparent red bg).
*   **Neutral/Inactive:** `.gray-tag` (gray border + semi-transparent gray bg).

### 5.3. Breadcrumbs
Every main content view (except the index dashboard) must define breadcrumbs via `ViewBag.Breadcrumbs` of type `List<(string Text, string Url)>` in the Razor code. The layout will automatically render it.

### 5.4. Pagination System
You **MUST** follow one of the two pagination patterns depending on the context:
1. **Standard Pagination:** For management/admin lists and grids (e.g. User Management, License Keys). Use Bootstrap 5 `.pagination` with `.rounded-0` square buttons. The active page button must have the background color of `var(--main-color)`.
2. **Infinite Scroll ("Xem thêm" Button):** For feed/exploration lists (e.g. public quizzes or comments). Do not use automatic scroll trigger to prevent blocking footer access. Instead, place a flat bordered button (`.btn .bg-white .border .rounded-0 .shadow-sm .highlight-hover`) labeled "Xem thêm" with a chevron-down icon. On click, trigger a loading spinner and append next-page items dynamically via API without reloading.

---

## ⚙️ Rule 6: Utilize Global UI Logic Helpers (Toast, Loading, and API Calls)
To prevent duplication and maintain system consistency, you **MUST** use the custom helpers defined in [site.js](file:///d:/projects/zlearn/ZLearn.Web/wwwroot/js/site.js):
*   **Toast/Messages:** Call `showMess(msg, isSuccess)` for all user feedback notifications. Never use `alert()` or third-party notification libraries.
*   **Confirm Dialogs:** Call `showConfirm(msg, callback)` for confirmations. Never use browser `confirm()`.
*   **Loading State:** Wrap async transactions with `showLoading()` and `hideLoading()` or let the API helpers handle them automatically.
*   **API Calls:** Use `getData(url, callback)`, `postJsonData(url, data, callback)`, or `putJsonData(url, data, callback)`. These helpers automatically toggle the loader and seamlessly handle 401 (trigger login dialog) and 403 (unauthorized/forbidden redirect) errors. Do not write raw `$.ajax` calls without proper error handling.

---

## ❌ Avoid These Common Mistakes
1.  **Do NOT use TailwindCSS** unless explicitly requested (the project uses Bootstrap 5 and Vanilla CSS).
2.  **Do NOT forget FontAwesome icons** in buttons and links. The design relies heavily on icons for scannability.
3.  **Do NOT use custom font sizes** like `font-size: 19px;` or arbitrary values. Use standard Bootstrap font sizing classes (`fs-*`, `fw-bold`, `fw-light`) or keep within the design specs.
4.  **Do NOT skip SEO tags** (Title, Meta Description, Keywords, Canonical URL, OpenGraph metadata) in any new `.cshtml` page.
5.  **Do NOT use native browser popups** (`alert()`, `confirm()`). Use `showMess()` and `showConfirm()`.
6.  **Do NOT write raw `$.ajax`** for standard CRUD calls. Use the built-in HTTP wrappers (`getData`, `postJsonData`, `putJsonData`).
