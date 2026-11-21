/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { AuthService } from '@app/shared';
import { createContent } from '../../../shared/services/contents.service.spec';
import { TestValues } from '../../../shared/state/_test-helpers';
import { PreviewButtonComponent } from './preview-button.component';

describe('PreviewButtonComponent', () => {
    let windowOpen: ReturnType<typeof vi.spyOn>;

    beforeEach(() => {
        windowOpen = vi.spyOn(window, 'open').mockReturnValue(null);
    });

    afterEach(() => {
        windowOpen.mockRestore();
    });

    it('should open preview url with interpolated values', () => {
        const button = createButton('https://squidex.io/preview/${createdBy}');

        button['follow']('Web');

        expect(windowOpen).toHaveBeenCalledWith('https://squidex.io/preview/creator12', '_blank');
    });

    ['javascript:alert(1)', ' JavaScript:alert(1)', 'data:text/html,<script>alert(1)</script>'].forEach(url => {
        it(`should not open unsafe preview url ${url}`, () => {
            const button = createButton(url);

            button['follow']('Web');

            expect(windowOpen).not.toHaveBeenCalled();
        });
    });
});

function createButton(previewUrl: string) {
    TestBed.configureTestingModule({
        providers: [
            { provide: AuthService, useValue: TestValues.authService.object },
        ],
    });

    const fixture = TestBed.createComponent(PreviewButtonComponent);

    fixture.componentRef.setInput('content', createContent(12));
    fixture.componentRef.setInput('schema', TestValues.createSchema({ previewUrls: { Web: previewUrl } }));

    return fixture.componentInstance;
}
