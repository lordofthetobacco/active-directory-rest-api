# Users

GET /user
GET /user/queryable?attributes=name,email,displayName
POST /user
GET /user/:user
GET /user/:user/queryable?attributes=name,email,displayName
GET /user/:user/exists
GET /user/:user/member-of/:group
POST /user/:user/authenticate
PUT /user/:user/password
PUT /user/:user/password-never-expires
PUT /user/:user/password-expires
PUT /user/:user/enable
PUT /user/:user/move
PUT /user/:user/unlock
DELETE /user/:user (Disables, doesn't delete)

# Groups

GET /group
GET /group/queryable?attributes=name,description,member
POST /group
GET /group/:group
GET /group/:group/queryable?attributes=name,description,member
GET /group/:group/exists
POST /group/:group/user/:user
DELETE /group/:group/user/:user (Deletes user from group)

# Organizational Units

GET /ou
GET /ou/queryable?attributes=name,description
POST /ou
GET /ou/:ou
GET /ou/:ou/queryable?attributes=name,description
GET /ou/:ou/exists

# Other

GET /other
GET /all
GET /find/users?name=&email=&ou=
GET /find/groups?name=&description=&ou=
GET /find/custom?filter=
GET /status

# Monitoring
GET /status