/* tslint:disable */
/* eslint-disable */
/**
 * ChatRoomsWeb
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: v1
 * 
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */

import { UserInfo } from './user-info';
 /**
 * 
 *
 * @export
 * @interface User
 */
export interface User {

    /**
     * @type {string}
     * @memberof User
     */
    username?: string | null;

    /**
     * @type {string}
     * @memberof User
     */
    hashedPassword?: string | null;

    /**
     * @type {UserInfo}
     * @memberof User
     */
    userInfo?: UserInfo;
}
