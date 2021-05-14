/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, Mock, Times } from 'typemoq';
import { TitleService } from './../internal';
import { TitleComponent } from './title.component';

describe('TitleComponent', () => {
    let titleService: IMock<TitleService>;
    let titleComponent: TitleComponent;

    beforeEach(() => {
        titleService = Mock.ofType<TitleService>();

        titleComponent = new TitleComponent(titleService.object);
    });

    it('should set title in service', () => {
        titleComponent.message = 'title1';

        expect().nothing();

        titleService.verify(x => x.push('title1', undefined), Times.once());
    });

    it('should replace title in title service', () => {
        titleComponent.message = 'title1';
        titleComponent.message = 'title2';

        expect().nothing();

        titleService.verify(x => x.push('title2', 'title1'), Times.once());
    });

    it('should remove title on destroy if set before', () => {
        titleComponent.message = 'title1';
        titleComponent.ngOnDestroy();

        expect().nothing();

        titleService.verify(x => x.pop(), Times.once());
    });

    it('should not remove title on destroy if not set before', () => {
        titleComponent.message = undefined!;
        titleComponent.ngOnDestroy();

        expect().nothing();

        titleService.verify(x => x.pop(), Times.never());
    });
});
