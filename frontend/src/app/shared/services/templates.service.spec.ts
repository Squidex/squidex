/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, Resource, TemplateDetailsDto, TemplateDto, TemplatesDto, TemplatesService } from '@app/shared/internal';
import { ResourceLinkDto } from '../model';

describe('TemplatesService', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        TemplatesService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get templates',
        inject([TemplatesService, HttpTestingController], (templatesService: TemplatesService, httpMock: HttpTestingController) => {
            let templates: TemplatesDto;

            templatesService.getTemplates().subscribe(result => {
                templates = result;
            });

            const req = httpMock.expectOne('http://service/p/api/templates');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                items: [
                    templateResponse(1),
                    templateResponse(2),
                ],
                _links: {},
            });

            expect(templates!).toEqual(new TemplatesDto({
                items: [
                    createTemplate(1),
                    createTemplate(2),
                ],
                _links: {},
            }));
        }));

    it('should make get request to get template',
        inject([TemplatesService, HttpTestingController], (templatesService: TemplatesService, httpMock: HttpTestingController) => {
            let template: TemplateDetailsDto;

            const resource: Resource = {
                _links: {
                    self: { method: 'GET', href: '/api/templates/my-template' },
                },
            };

            templatesService.getTemplate(resource).subscribe(result => {
                template = result;
            });

            const req = httpMock.expectOne('http://service/p/api/templates/my-template');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(templateDetailsResponse(1));

            expect(template!).toEqual(createTemplateDetails(1));
        }));

        function templateResponse(id: number, suffix = '') {
            const key = `${id}${suffix}`;

            return {
                name: `name${key}`,
                title: `Title ${key}`,
                description: `Description ${key}`,
                isStarter: id % 2 === 0,
                _links: {
                    self: { method: 'GET', href: `/templates/name${key}` },
                },
            };
        }

        function templateDetailsResponse(id: number, suffix = '') {
            const key = `${id}${suffix}`;

            return {
                details: `Details ${key}`,
                _links: {
                    self: { method: 'GET', href: `/templates/name${id}` },
                },
            };
        }
});

export function createTemplate(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new TemplateDto({
        name: `name${key}`,
        title: `Title ${key}`,
        description: `Description ${key}`,
        isStarter: id % 2 === 0,
        _links: {
            self: new ResourceLinkDto({ method: 'GET', href: `/templates/name${key}` }),
        },
    });
}


export function createTemplateDetails(id: number, suffix = '') {
    const key = `${id}${suffix}`;

    return new TemplateDetailsDto({
        details: `Details ${key}`,
        _links: {
            self: new ResourceLinkDto({ method: 'GET', href: `/templates/name${id}` }),
        },
    });
}