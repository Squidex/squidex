/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { inject, TestBed } from '@angular/core/testing';
import { ApiUrlConfig, ChangePlanDto, PlanChangedDto, PlanDto, PlansDto, ReferralInfoDto, Versioned, VersionTag } from '@app/shared';
import { TeamPlansService } from '../internal';

describe('TeamPlansService', () => {
    const version = new VersionTag('1');

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        TeamPlansService,
        { provide: ApiUrlConfig, useValue: new ApiUrlConfig('http://service/p/') },
    ],
});
    });

    afterEach(inject([HttpTestingController], (httpMock: HttpTestingController) => {
        httpMock.verify();
    }));

    it('should make get request to get team plans',
        inject([TeamPlansService, HttpTestingController], (plansService: TeamPlansService, httpMock: HttpTestingController) => {
            let plans: Versioned<PlansDto>;
            plansService.getPlans('my-team').subscribe(result => {
                plans = result;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/plans');

            expect(req.request.method).toEqual('GET');
            expect(req.request.headers.get('If-Match')).toBeNull();

            req.flush({
                currentPlanId: 'free',
                portalLink: 'link/to/portal',
                planOwner: '456',
                plans: [
                    {
                        id: 'free',
                        name: 'Free',
                        costs: '14 €',
                        confirmText: 'Change for 14 € per month?',
                        yearlyId: 'free_yearly',
                        yearlyCosts: '120 €',
                        yearlyConfirmText: 'Change for 120 € per year?',
                        maxApiBytes: 128,
                        maxApiCalls: 1000,
                        maxAssetSize: 1500,
                        maxContributors: 2500,
                    },
                    {
                        id: 'professional',
                        name: 'Professional',
                        costs: '18 €',
                        confirmText: 'Change for 18 € per month?',
                        yearlyId: 'professional_yearly',
                        yearlyCosts: '160 €',
                        yearlyConfirmText: 'Change for 160 € per year?',
                        maxApiBytes: 512,
                        maxApiCalls: 4000,
                        maxAssetSize: 5500,
                        maxContributors: 6500,
                    },
                ],
                referral: { code: 'CODE', earned: '0', condition: 'None' },
                locked: 'ManagedByTeam',
            }, {
                headers: {
                    etag: '2',
                },
            });

            expect(plans!).toEqual({
                payload: new PlansDto({
                    currentPlanId: 'free',
                    portalLink: 'link/to/portal',
                    planOwner: '456',
                    plans: [
                        new PlanDto({
                            id: 'free',
                            name: 'Free',
                            costs: '14 €',
                            confirmText: 'Change for 14 € per month?',
                            yearlyId: 'free_yearly',
                            yearlyCosts: '120 €',
                            yearlyConfirmText: 'Change for 120 € per year?',
                            maxApiBytes: 128,
                            maxApiCalls: 1000,
                            maxAssetSize: 1500,
                            maxContributors: 2500,
                        }),
                        new PlanDto({
                            id: 'professional',
                            name: 'Professional',
                            costs: '18 €',
                            confirmText: 'Change for 18 € per month?',
                            yearlyId: 'professional_yearly',
                            yearlyCosts: '160 €',
                            yearlyConfirmText: 'Change for 160 € per year?',
                            maxApiBytes: 512,
                            maxApiCalls: 4000,
                            maxAssetSize: 5500,
                            maxContributors: 6500,
                        }),
                    ],
                    referral: new ReferralInfoDto({ code: 'CODE', earned: '0', condition: 'None' }),
                    locked: 'ManagedByTeam',
                }),
                version: new VersionTag('2'),
            });
        }));

    it('should make put request to change plan',
        inject([TeamPlansService, HttpTestingController], (plansService: TeamPlansService, httpMock: HttpTestingController) => {
            const dto = new ChangePlanDto({ planId: 'enterprise' });

            let planChanged: PlanChangedDto;
            plansService.putPlan('my-team', dto, version).subscribe(result => {
                planChanged = result.payload;
            });

            const req = httpMock.expectOne('http://service/p/api/teams/my-team/plan');

            req.flush({ redirectUri: 'http://url' });

            expect(req.request.method).toEqual('PUT');
            expect(req.request.headers.get('If-Match')).toBe(version.value);

            expect(planChanged!).toEqual(new PlanChangedDto({ redirectUri: 'http://url' }));
        }));
});
