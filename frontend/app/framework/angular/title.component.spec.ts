/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Router } from '@angular/router';
import { IMock, It, Mock, Times } from 'typemoq';
import { TitleService } from './../internal';
import { TitleComponent } from './title.component';

describe('TitleComponent', () => {
    let titleService: IMock<TitleService>;
    let titleComponent: TitleComponent;
    let route: any = {};
    let router: IMock<Router>;

    beforeEach(() => {
        titleService = Mock.ofType<TitleService>();

        route = {};
        router = Mock.ofType<Router>();

        titleComponent = new TitleComponent(route, router.object, titleService.object);

        const tree: any = {};

        router.setup(x => x.createUrlTree(It.isAny(), { relativeTo: route }))
            .returns(() => tree);

        router.setup(x => x.serializeUrl(tree))
            .returns(() => 'my-url');

        titleService.setup(x => x.push(It.isAnyString(), It.isAny(), It.isAny()))
            .returns(() => 0);
    });

    it('should set title in service', () => {
        titleComponent.message = 'title1';
        titleComponent.ngOnChanges();

        expect().nothing();

        titleService.verify(x => x.push('title1', undefined, 'my-url'), Times.once());
    });

    it('should replace title in title service', () => {
        titleComponent.message = 'title1';
        titleComponent.ngOnChanges();

        titleComponent.message = 'title2';
        titleComponent.ngOnChanges();

        expect().nothing();

        titleService.verify(x => x.push('title2', 0, 'my-url'), Times.once());
    });

    it('should remove title on destroy if set before', () => {
        titleComponent.message = 'title1';
        titleComponent.ngOnChanges();
        titleComponent.ngOnDestroy();

        expect().nothing();

        titleService.verify(x => x.pop(), Times.once());
    });

    it('should not remove title on destroy if not set before', () => {
        titleComponent.message = undefined!;
        titleComponent.ngOnChanges();
        titleComponent.ngOnDestroy();

        expect().nothing();

        titleService.verify(x => x.pop(), Times.never());
    });
});
