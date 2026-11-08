/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, signal } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import { By } from "@angular/platform-browser";
import { FocusOnInitDirective } from "./focus-on-init.directive";

@Component({
    template: `<input [select]="select()" sqxFocusOnInit />`,
    imports: [FocusOnInitDirective],
})
class TestHostComponent {
    public readonly select = signal(false);
}

describe("FocusOnInitDirective", () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [TestHostComponent],
        });
    });

    it("should call focus on element if init", () => {
        const { focus, select } = render(false);

        expect(focus).toHaveBeenCalled();
        expect(select).not.toHaveBeenCalled();
    });

    it("should call select on element if init", () => {
        const { focus, select } = render(true);

        expect(focus).toHaveBeenCalled();
        expect(select).toHaveBeenCalled();
    });

    function render(shouldSelect: boolean) {
        const fixture = TestBed.createComponent(TestHostComponent);

        fixture.componentInstance.select.set(shouldSelect);

        const element = fixture.debugElement.query(
            By.directive(FocusOnInitDirective),
        );
        const directive = element.injector.get(FocusOnInitDirective);

        directive.scheduler = (action) => action();

        const input = element.nativeElement as HTMLInputElement;
        const focus = vi.spyOn(input, "focus");
        const select = vi.spyOn(input, "select");

        fixture.detectChanges();
        TestBed.tick();

        return { focus, select };
    }
});
