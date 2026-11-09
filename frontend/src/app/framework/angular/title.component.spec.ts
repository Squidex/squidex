/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { IMock, It, Mock, Times } from 'typemoq';
import { TitleService } from '../internal';
import { TitleComponent } from './title.component';

describe('TitleComponent', () => {
    let titleService: IMock<TitleService>;
    let titleFixture: ComponentFixture<TitleComponent>;
    let route: any = {};
    let router: IMock<Router>;

    beforeEach(() => {
        titleService = Mock.ofType<TitleService>();

        route = {};
        router = Mock.ofType<Router>();

        const tree: any = {};

        router.setup(x => x.createUrlTree(It.isAny(), { relativeTo: route }))
            .returns(() => tree);

        router.setup(x => x.serializeUrl(tree))
            .returns(() => 'my-url');

        titleService.setup(x => x.push(It.isAnyString(), It.isAny(), It.isAny()))
            .returns(() => 0);

        TestBed.configureTestingModule({
            providers: [
                { provide: ActivatedRoute, useValue: route },
                { provide: Router, useValue: router.object },
                { provide: TitleService, useValue: titleService.object },
            ],
        });

        titleFixture = TestBed.createComponent(TitleComponent);
    });

    it('should set title in service', () => {
        setMessage('title1');

        titleService.verify(x => x.push('title1', undefined, 'my-url'), Times.once());
    });

    it('should replace title in title service', () => {
        setMessage('title1');
        setMessage('title2');

        titleService.verify(x => x.push('title2', 0, 'my-url'), Times.once());
    });

    it('should remove title on destroy if set before', () => {
        setMessage('title1');
        titleFixture.destroy();

        titleService.verify(x => x.pop(), Times.once());
    });

    it('should not remove title on destroy if not set before', () => {
        titleFixture.destroy();

        titleService.verify(x => x.pop(), Times.never());
    });

    function setMessage(message: string) {
        titleFixture.componentRef.setInput('message', message);
        titleFixture.detectChanges();
    }
});
