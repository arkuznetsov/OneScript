pipeline {
    agent {
        label 'linux'
    }

    environment {
        DOCKER_USERNAME = "evilbeaver"
    }

    stages {
        stage('Build v1') {
            when { 
                anyOf {
                    branch 'release/latest'
                    expression { 
                        return env.TAG_NAME && env.TAG_NAME.startsWith('v1.')
                    }
                }
            }
            steps {
                script {
                    def dockerTag = env.TAG_NAME ? env.TAG_NAME : 'latest'
                    def imageName = "${env.DOCKER_USERNAME}/onescript:${dockerTag}"

                    docker.build(
                        imageName,
                        '--load -f install/builders/base-image/Dockerfile_v1 .'
                    ).push()
                }
            }
        }

        stage('Build v2') {
            when { 
                anyOf {
                    branch 'develop'
                    expression { 
                        return env.TAG_NAME && env.TAG_NAME.startsWith('v2.')
                    }
                }
            }
            steps {
                script {
                    def dockerTag = env.TAG_NAME ? env.TAG_NAME : 'dev'
                    def imageName = "${env.DOCKER_USERNAME}/onescript:${dockerTag}"

                    docker.build(
                        imageName,
                        '--load -f install/builders/base-image/Dockerfile_v2 .'
                    ).push()
                }
            }
        }
    }
}
