/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { AnalyticsService, ApiUrlConfig, AppDto, AppsService, DateTime, ErrorDto, Resource, ResourceLinks, Version } from '@app/shared/internal';
import { AppSettingsDto, AssetScriptsDto, AssetScriptsPayload, EditorDto, PatternDto } from './apps.service';

describe('AppsService', () => {
    const version = new Version('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [
                HttpClientTestingModule,
            ],
            providers: [
                AppsService,
                { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
                { provide: AnalyticsService, useValue: new AnalyticsService() },
            ],
        });
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get apps',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            let apps: ReadonlyArray<AppDto>;

            appsService.getApps().subscribe(result => {
                apps = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush([
                appResponse(12),
                appResponse(13),
            ]);

            expect(apps!).toEqual([createApp(12), createApp(13)]);
        }));

    it('should make get request to get app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            let app: AppDto;

            appsService.getApp('my-app').subscribe(result => {
                app = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(appResponse(12));

            expect(app!).toEqual(createApp(12));
        }));

    it('should make get request to get app settings',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            let settings: AppSettingsDto;

            appsService.getSettings('my-app').subscribe(result => {
                settings = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/settings');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(appSettingsResponse(12));

            expect(settings!).toEqual(createAppSettings(12));
        }));

    it('should make put request to update app settings',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/settings' },
                },
            };

            let settings: AppSettingsDto;

            appsService.putSettings('my-app', resource, {} as any, version).subscribe(result => {
                settings = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/settings');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(appSettingsResponse(12));

            expect(settings!).toEqual(createAppSettings(12));
        }));

    it('should make get request to get asset scripts',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            let settings: AssetScriptsDto;

            appsService.getAssetScripts('my-app').subscribe(result => {
                settings = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/scripts');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(assetScriptsResponse(12), {
                headers: {
                    etag: '2',
                },
            });

            expect(settings!).toEqual({ payload: createAssetScripts(12), version: new Version('2') });
        }));

    it('should make put request to update asset scripts',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app/assets/scripts' },
                },
            };

            let scripts: AssetScriptsDto;

            appsService.putAssetScripts('my-app', resource, {} as any, version).subscribe(result => {
                scripts = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/assets/scripts');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(assetScriptsResponse(12), {
                headers: {
                    etag: '2',
                },
            });

            expect(scripts!).toEqual({ payload: createAssetScripts(12), version: new Version('2') });
        }));

    it('should make post request to create app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const dto = { name: 'new-app' };

            let app: AppDto;

            appsService.postApp(dto).subscribe(result => {
                app = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush(appResponse(12));

            expect(app!).toEqual(createApp(12));
        }));

    it('should make put request to update app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    update: { method: 'PUT', href: '/api/apps/my-app' },
                },
            };

            let app: AppDto;

            appsService.putApp('my-app', resource, { }, version).subscribe(result => {
                app = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app');

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(appResponse(12));

            expect(app!).toEqual(createApp(12));
        }));

    it('should make post request to upload app image',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    'image/upload': { method: 'POST', href: '/api/apps/my-app/image' },
                },
            };

            let app: AppDto;

            appsService.postAppImage('my-app', resource, null!, version).subscribe(result => {
                app = <AppDto>result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/image');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(appResponse(12));

            expect(app!).toEqual(createApp(12));
        }));

    it('should return proper error if uploading app image failed with 413',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    'image/upload': { method: 'POST', href: '/api/apps/my-app/image' },
                },
            };

            let app: AppDto;
            let error: ErrorDto;

            appsService.postAppImage('my-app', resource, null!, version).subscribe(result => {
                app = <AppDto>result;
            }, e => {
                error = e;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/image');

            expect(req.request.method).toEqual('POST');
            expect(req.request.headers.get('If-Match')).toEqual(version.value);

            req.flush({}, { status: 413, statusText: 'Payload too large' });

            expect(app!).toBeUndefined();
            expect(error!).toEqual(new ErrorDto(413, 'i18n:apps.uploadImageTooBig'));
        }));

    it('should make delete request to remove app image',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    'image/delete': { method: 'DELETE', href: '/api/apps/my-app/image' },
                },
            };

            let app: AppDto;

            appsService.deleteAppImage('my-app', resource, version).subscribe(result => {
                app = result;
            });

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/image');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            req.flush(appResponse(12));

            expect(app!).toEqual(createApp(12));
        }));

    it('should make delete request to leave app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app/contributors/me' },
                },
            };

            appsService.deleteApp('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app/contributors/me');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    it('should make delete request to delete app',
        inject([AppsService, HttpTestingController], (appsService: AppsService, httpMock: HttpTestingController) => {
            const resource: Resource = {
                _links: {
                    delete: { method: 'DELETE', href: '/api/apps/my-app' },
                },
            };

            appsService.deleteApp('my-app', resource).subscribe();

            const req = httpMock.expectOne('http://service/p/api/apps/my-app');

            expect(req.request.method).toEqual('DELETE');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({});
        }));

    function appResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            id: `id${id}`,
            created: `${id % 1000 + 2000}-12-12T10:10:00Z`,
            createdBy: `creator${id}`,
            lastModified: `${id % 1000 + 2000}-11-11T10:10:00Z`,
            lastModifiedBy: `modifier${id}`,
            version: key,
            name: `app-name${key}`,
            label: `app-label${key}`,
            description: `app-description${key}`,
            permissions: ['Owner'],
            canAccessApi: id % 2 === 0,
            canAccessContent: id % 2 === 0,
            planName: 'Free',
            planUpgrade: 'Basic',
            roleProperties: createProperties(id),
            _links: {
                update: { method: 'PUT', href: `apps/${id}` },
            },
        };
    }

    function appSettingsResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            hideScheduler: false,
            patterns: [1, 2, 3].map(i => {
                const name = `pattern${i}${key}`;

                return { name, regex: `${name}_regex`, message: `${name}_message` };
            }),
            editors: [1, 2, 3].map(i => {
                const name = `editor${i}${key}`;

                return { name, url: `${name}_url` };
            }),
            version: key,
            _links: {
                update: { method: 'PUT', href: `apps/${id}/settings` },
            },
        };
    }

    function assetScriptsResponse(id: number, suffix = '') {
        const key = `${id}${suffix}`;

        return {
            update: key,
            _links: {
                update: { method: 'PUT', href: `apps/${id}/assets/scripts` },
            },
        };
    }
});

export function createApp(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `apps/${id}` },
    };

    const key = `${id}${suffix}`;

    return new AppDto(links,
        `id${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-12-12T10:10:00Z`), `creator${id}`,
        DateTime.parseISO(`${id % 1000 + 2000}-11-11T10:10:00Z`), `modifier${id}`,
        new Version(key),
        `app-name${key}`,
        `app-label${key}`,
        `app-description${key}`,
        ['Owner'],
        id % 2 === 0,
        id % 2 === 0,
        'Free', 'Basic',
        createProperties(id));
}

export function createAppSettings(id: number, suffix = '') {
    const links: ResourceLinks = {
        update: { method: 'PUT', href: `apps/${id}/settings` },
    };

    const key = `${id}${suffix}`;

    return new AppSettingsDto(links,
        false,
        [1, 2, 3].map(i => {
            const name = `pattern${i}${key}`;

            return new PatternDto(name, `${name}_regex`, `${name}_message`);
        }),
        [1, 2, 3].map(i => {
            const name = `editor${i}${key}`;

            return new EditorDto(name, `${name}_url`);
        }),
        new Version(key));
}

export function createAssetScripts(id: number, suffix = ''): AssetScriptsPayload {
    const key = `${id}${suffix}`;

    return {
        scripts: {
            update: key,
        },
        _links: {
            update: { method: 'PUT', href: `apps/${id}/assets/scripts` },
        },
        canUpdate: true,
    };
}

function createProperties(id: number) {
    const result = {};

    result[`property${id}`] = true;

    return result;
}
