const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');

(async () => {
    let submitHandler, submitted = 0, prevented = 0, alerts = 0, confirmed = false;
    const form = { addEventListener: (event, handler) => { if (event === 'submit') submitHandler = handler; } };
    const swal = { fire: async options => {
        assert.equal(options.focusCancel, true);
        assert.equal(options.confirmButtonText, 'Sil');
        assert.equal(options.cancelButtonText, 'İptal');
        return { isConfirmed: confirmed };
    } };
    const context = {
        document: {
            addEventListener() {}, getElementById: () => null, querySelector: () => null,
            querySelectorAll: selector => selector === 'form[class*="delete-"]' ? [form] : []
        },
        window: { Swal: swal }, Swal: swal,
        HTMLFormElement: { prototype: { submit() { assert.equal(this, form); submitted++; } } },
        alert() { alerts++; }
    };
    vm.runInNewContext(fs.readFileSync('wwwroot/admin/js/custom-admin.js', 'utf8'), context);
    const event = { preventDefault() { prevented++; } };
    await submitHandler(event);
    assert.equal(submitted, 0, 'Cancel must not submit');
    confirmed = true;
    await submitHandler(event);
    assert.equal(submitted, 1, 'Confirmation submits the original POST form');
    context.window.Swal = undefined;
    await submitHandler(event);
    assert.equal(submitted, 1, 'Missing SweetAlert must not silently delete');
    assert.equal(alerts, 1);
    assert.equal(prevented, 3);
    console.log('PASS: SweetAlert cancel, confirm, original form submission, and unavailable-library fallback');
})().catch(error => { console.error(error); process.exitCode = 1; });
